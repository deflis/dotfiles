using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ShellPrompt;

internal static class BatterySegment
{
    public static Segment? TryBuild()
    {
        if (OperatingSystem.IsWindows())
        {
            return TryWindows();
        }

        if (OperatingSystem.IsMacOS())
        {
            return TryMac();
        }

        if (OperatingSystem.IsLinux())
        {
            return TryLinux();
        }

        return null;
    }

    [SupportedOSPlatform("windows")]
    private static Segment? TryWindows()
    {
        if (!GetSystemPowerStatus(out var status))
        {
            throw new InvalidOperationException("GetSystemPowerStatus failed.");
        }

        // 255 = unknown / no battery
        if (status.BatteryLifePercent is 255 or > 100)
        {
            return null;
        }

        if ((status.BatteryFlag & 128) != 0)
        {
            return null;
        }

        var pct = status.BatteryLifePercent;
        var icon = (status.BatteryFlag & 8) != 0
            ? "\U000F0085"
            : status.ACLineStatus == 1
                ? "\U000F0079"
                : "\U000F007E";

        return new Segment(Ansi.PowerlineLeft, Palette.BatteryBg, Palette.BatteryFg, $" {icon} {pct}\uf295 ");
    }

    private static Segment? TryMac()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "pmset",
            ArgumentList = { "-g", "batt" },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var proc = Process.Start(psi)
                        ?? throw new InvalidOperationException("Failed to start pmset.");
        var output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        if (proc.ExitCode != 0)
        {
            return null;
        }

        var pctIdx = output.IndexOf('%');
        if (pctIdx < 0)
        {
            return null;
        }

        var start = pctIdx - 1;
        while (start >= 0 && char.IsDigit(output[start]))
        {
            start--;
        }

        start++;
        var pct = int.Parse(output[start..pctIdx]);
        var stateStart = output.IndexOf(';', pctIdx);
        if (stateStart < 0)
        {
            return null;
        }

        stateStart++;
        var stateEnd = output.IndexOf(';', stateStart);
        var state = (stateEnd < 0 ? output[stateStart..] : output[stateStart..stateEnd]).Trim().ToLowerInvariant();

        var icon = state.StartsWith("discharging", StringComparison.Ordinal) ? "\U000F007E"
            : state.StartsWith("charging", StringComparison.Ordinal) || state.StartsWith("finishing", StringComparison.Ordinal)
                ? "\U000F0085"
                : "\U000F0079";

        return new Segment(Ansi.PowerlineLeft, Palette.BatteryBg, Palette.BatteryFg, $" {icon} {pct}\uf295 ");
    }

    private static Segment? TryLinux()
    {
        var batDirs = Directory.Exists("/sys/class/power_supply")
            ? Directory.GetDirectories("/sys/class/power_supply", "BAT*")
            : [];
        if (batDirs.Length == 0)
        {
            return null;
        }

        var bat = batDirs[0];
        var capacityPath = Path.Combine(bat, "capacity");
        if (!File.Exists(capacityPath))
        {
            return null;
        }

        var pct = int.Parse(File.ReadAllText(capacityPath).Trim());
        var statusPath = Path.Combine(bat, "status");
        var state = File.Exists(statusPath)
            ? File.ReadAllText(statusPath).Trim().ToLowerInvariant()
            : string.Empty;

        var icon = state switch
        {
            "discharging" => "\U000F007E",
            "charging" => "\U000F0085",
            _ when state.StartsWith("finishing", StringComparison.Ordinal) => "\U000F0085",
            _ => "\U000F0079",
        };

        return new Segment(Ansi.PowerlineLeft, Palette.BatteryBg, Palette.BatteryFg, $" {icon} {pct}\uf295 ");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemPowerStatus
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte Reserved1;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }

    [DllImport("kernel32.dll")]
    [SupportedOSPlatform("windows")]
    private static extern bool GetSystemPowerStatus(out SystemPowerStatus sps);
}
