using System.Text;

namespace ShellPrompt;

internal static class PromptRenderer
{
    public static string Render(
        string osText,
        Rgb osBg,
        Rgb osFg,
        string pathText,
        IReadOnlyList<Segment> rightSegments,
        int exitCode,
        bool isRoot,
        int columns)
    {
        var left = BuildLeft(osText, osBg, osFg, pathText);
        var line2 = BuildLine2(exitCode, isRoot);

        var segs = rightSegments.ToList();
        string right;
        int pad;
        while (true)
        {
            right = BuildRight(segs);
            var leftW = Ansi.VisibleWidth(left);
            var rightW = Ansi.VisibleWidth(right);
            pad = columns - leftW - rightW;
            if (pad >= 0 || segs.Count == 0)
            {
                break;
            }

            segs.RemoveAt(segs.Count - 1);
        }

        if (segs.Count == 0)
        {
            return left + "\n" + line2;
        }

        if (pad < 0)
        {
            pad = 0;
        }

        return left + new string(' ', pad) + right + "\n" + line2;
    }

    private static string BuildLeft(string osText, Rgb osBg, Rgb osFg, string pathText)
    {
        var sb = new StringBuilder();
        Ansi.FgBg(sb, osFg, osBg);
        sb.Append(osText);
        Ansi.FgBg(sb, osBg, Palette.LightBlue);
        sb.Append(Ansi.PowerlineRight);

        Ansi.FgBg(sb, Palette.DarkBlue, Palette.LightBlue);
        sb.Append(pathText);
        sb.Append(Ansi.Reset);
        Ansi.Fg(sb, Palette.LightBlue);
        sb.Append(Ansi.PowerlineRight);
        sb.Append(Ansi.Reset);
        return sb.ToString();
    }

    private static string BuildRight(IReadOnlyList<Segment> segs)
    {
        if (segs.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        Rgb? prev = null;
        foreach (var seg in segs)
        {
            if (prev is null)
            {
                Ansi.Fg(sb, seg.Background);
                sb.Append(seg.Separator);
            }
            else
            {
                Ansi.FgBg(sb, seg.Background, prev.Value);
                sb.Append(seg.Separator);
            }

            Ansi.FgBg(sb, seg.Foreground, seg.Background);
            sb.Append(seg.Text);
            sb.Append(Ansi.Reset);
            prev = seg.Background;
        }

        Ansi.Fg(sb, prev!.Value);
        sb.Append(Ansi.PowerlineRight);
        sb.Append(Ansi.Reset);
        return sb.ToString();
    }

    private static string BuildLine2(int exitCode, bool isRoot)
    {
        var sb = new StringBuilder();
        Ansi.Fg(sb, exitCode == 0 ? Palette.Green : Palette.Red);
        sb.Append('\ue285');
        sb.Append(Ansi.Reset);
        if (isRoot)
        {
            Ansi.Fg(sb, Palette.LightRed);
            sb.Append("# ");
        }
        else
        {
            Ansi.Fg(sb, Palette.White);
            sb.Append("$ ");
        }

        sb.Append(Ansi.Reset);
        return sb.ToString();
    }
}
