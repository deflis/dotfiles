using System.Globalization;
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
        for (var i = 0; i < s.Length;)
        {
            if (s[i] == '\x1b' && i + 1 < s.Length && s[i + 1] == '[')
            {
                i += 2;
                while (i < s.Length && s[i] is not (>= 'A' and <= 'Z') and not (>= 'a' and <= 'z'))
                {
                    i++;
                }

                if (i < s.Length)
                {
                    i++;
                }

                continue;
            }

            var status = Rune.DecodeFromUtf16(s[i..], out var rune, out var consumed);
            if (status != System.Buffers.OperationStatus.Done || consumed <= 0)
            {
                width++;
                i++;
                continue;
            }

            width += RuneDisplayWidth(rune);
            i += consumed;
        }

        return width;
    }

    private static int RuneDisplayWidth(Rune rune)
    {
        var cat = Rune.GetUnicodeCategory(rune);
        if (cat is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark or UnicodeCategory.Format
            or UnicodeCategory.Control)
        {
            return 0;
        }

        var v = rune.Value;
        // Private Use (Nerd Fonts) — treat as single column like most terminals with patched fonts
        if (v is >= 0xE000 and <= 0xF8FF or >= 0xF0000 and <= 0xFFFFD)
        {
            return 1;
        }

        if (IsWide(v))
        {
            return 2;
        }

        return 1;
    }

    private static bool IsWide(int cp) =>
        cp is (>= 0x1100 and <= 0x115F)
            or 0x2329 or 0x232A
            or (>= 0x2E80 and <= 0xA4CF)
            or (>= 0xAC00 and <= 0xD7A3)
            or (>= 0xF900 and <= 0xFAFF)
            or (>= 0xFE10 and <= 0xFE19)
            or (>= 0xFE30 and <= 0xFE6F)
            or (>= 0xFF00 and <= 0xFF60)
            or (>= 0xFFE0 and <= 0xFFE6)
            or (>= 0x1F300 and <= 0x1FAFF)
            or (>= 0x20000 and <= 0x3FFFD);
}
