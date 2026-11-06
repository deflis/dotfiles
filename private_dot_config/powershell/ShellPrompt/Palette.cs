namespace ShellPrompt;

internal static class Palette
{
    public static readonly Rgb Black = Rgb.Parse("#111111");
    public static readonly Rgb Blue = Rgb.Parse("#3261ab");
    public static readonly Rgb Cyan = Rgb.Parse("#007fb1");
    public static readonly Rgb DarkBlue = Rgb.Parse("#142744");
    public static readonly Rgb DarkGreen = Rgb.Parse("#0e4506");
    public static readonly Rgb Green = Rgb.Parse("#23ac0e");
    public static readonly Rgb LightBlue = Rgb.Parse("#d5e0f1");
    public static readonly Rgb LightRed = Rgb.Parse("#f6d4d8");
    public static readonly Rgb Lime = Rgb.Parse("#a4c520");
    public static readonly Rgb Magenta = Rgb.Parse("#bf1e56");
    public static readonly Rgb Orange = Rgb.Parse("#da5019");
    public static readonly Rgb Red = Rgb.Parse("#c7243a");
    public static readonly Rgb White = Rgb.Parse("#d6deeb");
    public static readonly Rgb YellowOrange = Rgb.Parse("#edad0b");

    public static readonly Rgb BatteryBg = Rgb.Parse("#8f43f3");
    public static readonly Rgb BatteryFg = Rgb.Parse("#ffffff");
    public static readonly Rgb NodeBg = Rgb.Parse("#303030");
    public static readonly Rgb NodeFg = Rgb.Parse("#3C873A");
    public static readonly Rgb NpmIcon = Rgb.Parse("#cc3a3a");
    public static readonly Rgb YarnIcon = Rgb.Parse("#348cba");
    public static readonly Rgb PerlBg = Rgb.Parse("#5F8CC2");
    public static readonly Rgb PerlFg = Rgb.Parse("#000000");
    public static readonly Rgb GoBg = Rgb.Parse("#ffffff");
    public static readonly Rgb GoFg = Rgb.Parse("#06aad5");
    public static readonly Rgb RustBg = Rgb.Parse("#dea584");
    public static readonly Rgb RustFg = Rgb.Parse("#000000");
    public static readonly Rgb RubyBg = Rgb.Parse("#ffffff");
    public static readonly Rgb RubyFg = Rgb.Parse("#9c1006");
    public static readonly Rgb ShellBg = Rgb.Parse("#d6deeb");
    public static readonly Rgb ShellFg = Rgb.Parse("#011627");
}

internal readonly record struct Rgb(byte R, byte G, byte B)
{
    public static Rgb Parse(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#')
        {
            throw new ArgumentException($"Invalid color: {hex}", nameof(hex));
        }

        return new Rgb(
            Convert.ToByte(hex[1..3], 16),
            Convert.ToByte(hex[3..5], 16),
            Convert.ToByte(hex[5..7], 16));
    }
}
