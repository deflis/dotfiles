namespace ShellPrompt;

internal static class OsSegment
{
    private static readonly (string Text, Rgb Bg, Rgb Fg) Windows =
        ("\U000F0372 ", Palette.Blue, Palette.White);

    public static (string Text, Rgb Bg, Rgb Fg) Get(bool isSsh, string hostName)
    {
        var (text, bg, fg) = Windows;
        if (isSsh)
        {
            text += hostName;
        }

        return (text, bg, fg);
    }
}
