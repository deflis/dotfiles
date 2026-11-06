using System.Runtime.InteropServices;

namespace ShellPrompt;

internal static class OsSegment
{
    private static readonly Lazy<(string Text, Rgb Bg, Rgb Fg)> Cached = new(Detect);

    public static (string Text, Rgb Bg, Rgb Fg) Get(bool isSsh, string hostName)
    {
        var (text, bg, fg) = Cached.Value;
        if (isSsh)
        {
            text += hostName;
        }

        return (text, bg, fg);
    }

    private static (string Text, Rgb Bg, Rgb Fg) Detect()
    {
        if (OperatingSystem.IsWindows())
        {
            return ("\U000F0372 ", Palette.Blue, Palette.White);
        }

        if (OperatingSystem.IsMacOS())
        {
            return ("\uf179 ", Palette.Black, Palette.White);
        }

        if (OperatingSystem.IsLinux())
        {
            var (icon, bg, fg) = LinuxFlavor();
            var text = icon + " ";
            if (IsWsl())
            {
                text += "\U000F0372 ";
            }

            return (text, bg, fg);
        }

        return ("\uf17c ", Palette.Black, Palette.White);
    }

    private static (string Icon, Rgb Bg, Rgb Fg) LinuxFlavor()
    {
        var id = ReadOsReleaseId();
        return id switch
        {
            "ubuntu" => ("\uf31b", Palette.Orange, Palette.White),
            "arch" => ("\uf303", Palette.Cyan, Palette.White),
            "debian" => ("\uf306", Palette.Magenta, Palette.White),
            "raspbian" => ("\uf315", Palette.Magenta, Palette.White),
            "manjaro" => ("\uf312", Palette.DarkGreen, Palette.Green),
            "centos" => ("\uf304", Palette.YellowOrange, Palette.Black),
            _ => ("\uf17c", Palette.Black, Palette.White),
        };
    }

    private static string ReadOsReleaseId()
    {
        const string path = "/etc/os-release";
        if (!File.Exists(path))
        {
            return string.Empty;
        }

        foreach (var line in File.ReadLines(path))
        {
            if (line.StartsWith("ID=", StringComparison.Ordinal))
            {
                return line[3..].Trim().Trim('"');
            }
        }

        return string.Empty;
    }

    private static bool IsWsl()
    {
        var release = RuntimeInformation.OSDescription;
        return release.Contains("microsoft", StringComparison.OrdinalIgnoreCase)
               || File.Exists("/proc/sys/fs/binfmt_misc/WSLInterop");
    }
}
