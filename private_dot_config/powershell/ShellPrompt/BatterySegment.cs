using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ShellPrompt;

[SupportedOSPlatform("windows")]
internal static class BatterySegment
{
    public static Segment? TryBuild()
    {
        if (!GetSystemPowerStatus(out var status))
        {
            return null;
        }

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
    private static extern bool GetSystemPowerStatus(out SystemPowerStatus sps);
}
