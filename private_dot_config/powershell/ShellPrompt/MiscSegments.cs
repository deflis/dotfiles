using System.Security.Principal;

namespace ShellPrompt;

internal static class MiscSegments
{
    public static Segment? ExecutionTime(long executionMs)
    {
        if (executionMs < 0)
        {
            return null;
        }

        return new Segment(Ansi.PowerlineLeft, Palette.DarkBlue, Palette.White, FormatRoundrock(executionMs));
    }

    public static Segment Shell(string shellName) =>
        new(Ansi.PowerlineLeft, Palette.ShellBg, Palette.ShellFg, $"\uf489 {shellName}");

    public static Segment Time(DateTimeOffset now) =>
        new(Ansi.PowerlineLeft, Palette.Blue, Palette.White, $"\U000F0954 {now:HH:mm:ss}");

    public static string FormatRoundrock(long ms)
    {
        var remaining = ms;
        var h = remaining / 3_600_000;
        remaining %= 3_600_000;
        var m = remaining / 60_000;
        remaining %= 60_000;
        var s = remaining / 1_000;
        remaining %= 1_000;

        var parts = new List<string>();
        if (h > 0)
        {
            parts.Add($"{h}h");
        }

        if (h > 0 || m > 0)
        {
            parts.Add($"{m}m");
        }

        if (h > 0 || m > 0 || s > 0)
        {
            parts.Add($"{s}s");
        }

        parts.Add($"{remaining}ms");
        return string.Join(' ', parts);
    }

    public static bool IsSsh() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SSH_CONNECTION"))
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SSH_CLIENT"))
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SSH_TTY"));

    public static bool IsRoot()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }
}
