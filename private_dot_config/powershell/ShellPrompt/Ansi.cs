using System.Text;

namespace ShellPrompt;

internal static class Ansi
{
    public const string Reset = "\x1b[0m";
    public const char PowerlineRight = '\ue0b4';
    public const char PowerlineLeft = '\ue0b2';
    public const char PowerlineLeftRound = '\ue0b6';
    public const char PathSeparator = '\ue0b1';

    public static void Fg(StringBuilder sb, Rgb c) =>
        sb.Append("\x1b[38;2;").Append(c.R).Append(';').Append(c.G).Append(';').Append(c.B).Append('m');

    public static void Bg(StringBuilder sb, Rgb c) =>
        sb.Append("\x1b[48;2;").Append(c.R).Append(';').Append(c.G).Append(';').Append(c.B).Append('m');

    public static void FgBg(StringBuilder sb, Rgb fg, Rgb bg)
    {
        Fg(sb, fg);
        Bg(sb, bg);
    }

    public static int VisibleWidth(ReadOnlySpan<char> s)
    {
        var width = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == '\x1b' && i + 1 < s.Length && s[i + 1] == '[')
            {
                i += 2;
                while (i < s.Length && s[i] is not (>= 'A' and <= 'Z') and not (>= 'a' and <= 'z'))
                {
                    i++;
                }

                continue;
            }

            width++;
        }

        return width;
    }
}
