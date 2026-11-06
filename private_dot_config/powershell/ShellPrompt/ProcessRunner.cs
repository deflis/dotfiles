using System.ComponentModel;
using System.Diagnostics;

namespace ShellPrompt;

internal static class ProcessRunner
{
    public static string? Run(
        string cwd,
        string fileName,
        string[] args,
        IReadOnlyDictionary<string, string>? env = null)
    {
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

        try
        {
            using var proc = Process.Start(psi);
            if (proc is null)
            {
                return null;
            }

            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var stderrTask = proc.StandardError.ReadToEndAsync();
            Task.WaitAll(stdoutTask, stderrTask);
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                return null;
            }

            var stdout = stdoutTask.Result;
            return string.IsNullOrWhiteSpace(stdout) ? null : stdout;
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or Win32Exception)
        {
            return null;
        }
    }
}
