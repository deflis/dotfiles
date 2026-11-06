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
    private static string? _pendingErrorCwd;
    private static int _redrawDepth;
    private const int MaxCachedCwds = 64;

    public static void EnterRedraw() => Interlocked.Increment(ref _redrawDepth);

    public static void ExitRedraw() => Interlocked.Decrement(ref _redrawDepth);

    public static IReadOnlyList<Segment> GetAndRefresh(string cwd)
    {
        CancellationTokenSource? cts = null;
        Exception? sticky;
        IReadOnlyList<Segment> segs;

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

            if (Volatile.Read(ref _redrawDepth) == 0)
            {
                cts = new CancellationTokenSource();
                var previous = _cts;
                _cts = cts;
                CancelQuiet(previous);
            }

            sticky = StickyError(cwd);
            segs = _displaySegs;
        }

        if (cts is not null)
        {
            var workerCts = cts;
            _ = Task.Run(() =>
            {
                try
                {
                    Worker(cwd, workerCts.Token);
                }
                finally
                {
                    workerCts.Dispose();
                }
            });
        }

        lock (Gate)
        {
            sticky = StickyError(cwd);
            if (sticky is null)
            {
                segs = _displaySegs;
            }
        }

        if (sticky is not null)
        {
            throw new InvalidOperationException("Async prompt segment collection failed.", sticky);
        }

        return segs;
    }

    private static void Worker(string cwd, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            var segs = new List<Segment>();
            var git = GitSegment.TryBuild(cwd, token);
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

            segs.AddRange(LangSegments.Build(cwd, token));
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

                var clearedError = _pendingError is not null
                                   && string.Equals(_pendingErrorCwd, cwd, StringComparison.OrdinalIgnoreCase);
                if (clearedError)
                {
                    _pendingError = null;
                    _pendingErrorCwd = null;
                }

                var sameCwd = _displayCwd is not null
                              && string.Equals(_displayCwd, cwd, StringComparison.OrdinalIgnoreCase);
                if (!sameCwd)
                {
                    return;
                }

                shouldRedraw = clearedError || !SegmentsEqual(_displaySegs, segs);
                _displaySegs = segs;
            }

            if (shouldRedraw)
            {
                PromptRefresh.Request();
            }
        }
        catch (OperationCanceledException)
        {
            // 新しいプロンプト描画で置き換えられた。結果は採用しない。
        }
        catch (Exception ex)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            lock (Gate)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                _pendingError = ex;
                _pendingErrorCwd = cwd;
            }

            PromptRefresh.Request();
        }
    }

    private static Exception? StickyError(string cwd)
    {
        if (_pendingError is null
            || !string.Equals(_pendingErrorCwd, cwd, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return _pendingError;
    }

    private static void CancelQuiet(CancellationTokenSource? cts)
    {
        if (cts is null)
        {
            return;
        }

        try
        {
            cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 前のワーカーがすでに終了してソースを破棄した。
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
