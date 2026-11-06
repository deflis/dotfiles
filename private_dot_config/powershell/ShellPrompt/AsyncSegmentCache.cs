using System.Collections.Concurrent;

namespace ShellPrompt;

/// <summary>
/// zsh の _dp_async_* 相当: プロンプト毎に前回ワーカーをキャンセルして再実行し、
/// cwd 別キャッシュを先に表示してから非同期更新→InvokePrompt で再描画する。
/// </summary>
internal static class AsyncSegmentCache
{
    private static readonly ConcurrentDictionary<string, IReadOnlyList<Segment>> Cache = new(StringComparer.Ordinal);
    private static readonly object Gate = new();
    private static CancellationTokenSource? _cts;
    private static string? _displayCwd;
    private static IReadOnlyList<Segment> _displaySegs = Array.Empty<Segment>();
    private static Exception? _pendingError;

    public static IReadOnlyList<Segment> GetAndRefresh(string cwd)
    {
        if (_pendingError is { } err)
        {
            _pendingError = null;
            throw new InvalidOperationException("Async prompt segment collection failed.", err);
        }

        lock (Gate)
        {
            if (!string.Equals(_displayCwd, cwd, StringComparison.Ordinal))
            {
                _displayCwd = cwd;
                _displaySegs = Cache.TryGetValue(cwd, out var cached)
                    ? cached
                    : Array.Empty<Segment>();
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
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

            Cache[cwd] = segs;

            bool shouldRedraw;
            lock (Gate)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (!string.Equals(_displayCwd, cwd, StringComparison.Ordinal))
                {
                    return;
                }

                shouldRedraw = !SegmentsEqual(_displaySegs, segs);
                _displaySegs = segs;
            }

            if (shouldRedraw)
            {
                PromptRefresh.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // 新しいプロンプト描画で置き換えられた
        }
        catch (Exception ex)
        {
            _pendingError = ex;
            try
            {
                PromptRefresh.Invoke();
            }
            catch
            {
                // 次回 Get-ShellPrompt で再送出
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
