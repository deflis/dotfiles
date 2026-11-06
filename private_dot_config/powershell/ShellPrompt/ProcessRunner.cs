using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace ShellPrompt;

/// <summary>
/// <see cref="ProcessResult"/> が null のときはプロセスを起動できなかった（ツール未導入）。
/// 終了コードは呼び出し側が解釈する。
/// </summary>
internal readonly record struct ProcessResult(int ExitCode, string Stdout, string Stderr);

internal static class ProcessRunner
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public static ProcessResult? Run(
        string cwd,
        string fileName,
        string[] args,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string>? env = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = cwd,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        if (env is not null)
        {
            foreach (var (key, value) in env)
            {
                psi.Environment[key] = value;
            }
        }

        Process? proc;
        try
        {
            proc = Process.Start(psi);
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or Win32Exception)
        {
            return null;
        }

        if (proc is null)
        {
            return null;
        }

        using (proc)
        {
            return Drain(proc, fileName, cancellationToken).GetAwaiter().GetResult();
        }
    }

    private static async Task<ProcessResult> Drain(
        Process proc,
        string fileName,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(Timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var stdoutTask = proc.StandardOutput.ReadToEndAsync(readCts.Token);
        var stderrTask = proc.StandardError.ReadToEndAsync(readCts.Token);
        var exitTask = proc.WaitForExitAsync(readCts.Token);
        var done = Task.WhenAll(exitTask, stdoutTask, stderrTask);
        try
        {
            // WaitAsync のタイマーで打ち切る。プロセス側がトークンを無視してもハングしない。
            await done.WaitAsync(linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (done.IsCompletedSuccessfully)
        {
            // 期限と完了が重なった。取れた出力を使う。
        }
        catch (Exception ex) when (!done.IsCompletedSuccessfully)
        {
            readCts.Cancel();
            TryKill(proc);
            await Abandon(exitTask, stdoutTask, stderrTask).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (ex is OperationCanceledException)
            {
                throw new TimeoutException(
                    $"'{fileName}' did not exit within {(int)Timeout.TotalSeconds} seconds.");
            }

            ExceptionDispatchInfo.Capture(ex).Throw();
        }

        return new ProcessResult(
            proc.ExitCode,
            await stdoutTask.ConfigureAwait(false),
            await stderrTask.ConfigureAwait(false));
    }

    private static async Task Abandon(params Task[] tasks)
    {
        var pending = Task.WhenAll(tasks);
        var finished = await Task.WhenAny(pending, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
        if (!ReferenceEquals(finished, pending))
        {
            _ = pending.ContinueWith(
                static task => { _ = task.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            return;
        }

        try
        {
            await pending.ConfigureAwait(false);
        }
        catch
        {
            // Kill 後のパイプ切断やキャンセル。呼び出し側の例外を優先する。
        }
    }

    private static void TryKill(Process proc)
    {
        try
        {
            if (!proc.HasExited)
            {
                proc.Kill(entireProcessTree: true);
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or NotSupportedException)
        {
            // すでに終了している
        }
    }
}
