using System.Collections.Concurrent;

namespace ShellPrompt;

/// <summary>
/// zsh の _dp_async_* 相当: プロンプト毎に前回ワーカーをキャンセルして再実行し、
/// cwd 別キャッシュを先に表示してから非同期更新→再描画要求する。
/// </summary>
internal static class AsyncSegmentCache
{
    private static readonly ConcurrentDictionary<string, IReadOnlyList<Segment>> Cache =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly object Gate = new();
    private static CancellationTokenSource? _cts;
    private static string? _displayCwd;
    private static IReadOnlyList<Segment> _displaySegs = Array.Empty<Segment>();
    private static Exception? _pendingError;
    private const int MaxCachedCwds = 64;

    public static IReadOnlyList<Segment> GetAndRefresh(string cwd)
    {
        var err = Interlocked.Exchange(ref _pendingError, null);
        if (err is not null)
        {
            throw new InvalidOperationException("Async prompt segment collection failed.", err);
        }

        lock (Gate)
        {
            var sameCwd = _displayCwd is not null
                          && string.Equals(_displayCwd, cwd, StringComparison.OrdinalIgnoreCase);

            if (!sameCwd)
            {
                _displayCwd = cwd;
                _displaySegs = Cache.TryGetValue(cwd, out var cached)
                    ? cached
                    : Array.Empty<Segment>();
            }

            var old = _cts;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            old?.Cancel();
            old?.Dispose();

            var snapshot = _displaySegs;
            _ = Task.Run(() => Worker(cwd, token), token);
            return snapshot;
        }
    }

    private static void Worker(string cwd, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            var segs = new List<Segment>();
            var git = GitSegment.TryBuild(cwd);
            token.ThrowIfCancellationRequested();
            if (git is { } g)
            {
                segs.Add(g);
            }

            var battery = BatterySegment.TryBuild();
            token.ThrowIfCancellationRequested();
            if (battery is { } b)
            {
                segs.Add(b);
            }

            segs.AddRange(LangSegments.Build(cwd));
            token.ThrowIfCancellationRequested();

            bool shouldRedraw;
            lock (Gate)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                Cache[cwd] = segs;
                TrimCache(cwd);

                var sameCwd = _displayCwd is not null
                              && string.Equals(_displayCwd, cwd, StringComparison.OrdinalIgnoreCase);
                if (!sameCwd)
                {
                    return;
                }

                shouldRedraw = !SegmentsEqual(_displaySegs, segs);
                _displaySegs = segs;
            }

            if (shouldRedraw)
            {
                PromptRefresh.Request();
            }
        }
        catch (OperationCanceledException)
        {
            // 新しいプロンプト描画で置き換えられた
        }
        catch (Exception ex)
        {
            Interlocked.Exchange(ref _pendingError, ex);
            PromptRefresh.Request();
        }
    }

    private static void TrimCache(string keepCwd)
    {
        if (Cache.Count <= MaxCachedCwds)
        {
            return;
        }

        foreach (var key in Cache.Keys)
        {
            if (!string.Equals(key, keepCwd, StringComparison.OrdinalIgnoreCase))
            {
                Cache.TryRemove(key, out _);
                if (Cache.Count <= MaxCachedCwds / 2)
                {
                    break;
                }
            }
        }
    }

    private static bool SegmentsEqual(IReadOnlyList<Segment> a, IReadOnlyList<Segment> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        for (var i = 0; i < a.Count; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }
}
