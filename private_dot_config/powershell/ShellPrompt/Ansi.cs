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
        // Private Use (Nerd Fonts) — single column, same as a typical patched-font terminal.
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
            // U+303F and U+4DC0–U+4DFF sit inside the CJK span but are not wide.
            or (>= 0x2E80 and <= 0x303E)
            or (>= 0x3040 and <= 0x4DBF)
            or (>= 0x4E00 and <= 0xA4CF)
            or (>= 0xA960 and <= 0xA97F)
            or (>= 0xAC00 and <= 0xD7A3)
            or (>= 0xF900 and <= 0xFAFF)
            or (>= 0xFE10 and <= 0xFE19)
            or (>= 0xFE30 and <= 0xFE6F)
            or (>= 0xFF00 and <= 0xFF60)
            or (>= 0xFFE0 and <= 0xFFE6)
            or (>= 0x1F200 and <= 0x1F2FF)
            or (>= 0x1F300 and <= 0x1F64F)
            or (>= 0x1F680 and <= 0x1F6FF)
            // U+1F650–U+1F67F, most of U+1F700–U+1F8FF, and U+1FA00–U+1FA6F are neutral.
            or (>= 0x1F7E0 and <= 0x1F7EB)
            or 0x1F7DA or 0x1F7F0
            or (>= 0x1F900 and <= 0x1F9FF)
            or (>= 0x1FA70 and <= 0x1FAFF)
            or (>= 0x20000 and <= 0x3FFFD)
            || IsWideSymbol(cp);

    private static bool IsWideSymbol(int cp) =>
        cp is (>= 0x231A and <= 0x231B)
            or (>= 0x23E9 and <= 0x23EC)
            or 0x23F0 or 0x23F3
            or (>= 0x25FD and <= 0x25FE)
            or (>= 0x2614 and <= 0x2615)
            or (>= 0x2630 and <= 0x2637)
            or (>= 0x2648 and <= 0x2653)
            or 0x267F
            or (>= 0x268A and <= 0x268F)
            or 0x2693
            or 0x26A1
            or (>= 0x26AA and <= 0x26AB)
            or (>= 0x26BD and <= 0x26BE)
            or (>= 0x26C4 and <= 0x26C5)
            or 0x26CE
            or 0x26D4
            or 0x26EA
            or (>= 0x26F2 and <= 0x26F3)
            or 0x26F5
            or 0x26FA
            or 0x26FD
            or 0x2705
            or (>= 0x270A and <= 0x270B)
            or 0x2728
            or 0x274C
            or 0x274E
            or (>= 0x2753 and <= 0x2755)
            or 0x2757
            or (>= 0x2795 and <= 0x2797)
            or 0x27B0
            or 0x27BF
            or (>= 0x2B1B and <= 0x2B1C)
            or 0x2B50
            or 0x2B55;
}
