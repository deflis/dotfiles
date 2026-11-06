using System.Text;

namespace ShellPrompt;

internal static class PathSegment
{
    private static readonly string Gap = CreateGap();

    public static string Format(string cwd, string homeDir)
    {
        var path = cwd;
        if (!string.IsNullOrEmpty(homeDir))
        {
            homeDir = homeDir.TrimEnd('\\', '/');
        }

        if (!string.IsNullOrEmpty(homeDir) && IsHomePrefix(path, homeDir))
        {
            path = "~" + path[homeDir.Length..];
        }

        path = path.Replace('\\', '/');
        return Render(SplitNormalized(path));
    }

    public static string Shorten(string pathText, int maxVisibleWidth)
    {
        if (maxVisibleWidth < 0)
        {
            maxVisibleWidth = 0;
        }

        if (Ansi.VisibleWidth(pathText) <= maxVisibleWidth)
        {
            return pathText;
        }

        if (maxVisibleWidth < 3)
        {
            return new string(' ', maxVisibleWidth);
        }

        return Render(Shrink(Parse(pathText), maxVisibleWidth));
    }

    private static bool IsHomePrefix(string path, string homeDir)
    {
        if (!path.StartsWith(homeDir, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (path.Length == homeDir.Length)
        {
            return true;
        }

        var next = path[homeDir.Length];
        return next is '/' or '\\';
    }

    private static List<string> SplitNormalized(string path)
    {
        if (path is "/" or "~")
        {
            return [path];
        }

        if (path.StartsWith("//", StringComparison.Ordinal))
        {
            var segs = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segs.Length >= 2)
            {
                var parts = new List<string> { "//" + segs[0] + "/" + segs[1] };
                for (var i = 2; i < segs.Length; i++)
                {
                    parts.Add(segs[i]);
                }

                return parts;
            }

            return segs.Length == 1 ? ["//" + segs[0]] : ["//"];
        }

        if (IsDrive(path))
        {
            if (path.Length == 2)
            {
                return [path];
            }

            if (path[2] == '/')
            {
                if (path.Length == 3)
                {
                    return [path[..2] + "/"];
                }

                var parts = new List<string> { path[..2] };
                foreach (var part in path[3..].Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    parts.Add(part);
                }

                return parts;
            }
        }

        var list = new List<string>();
        if (path.StartsWith('/'))
        {
            list.Add("/");
            path = path[1..];
        }
        else if (path.StartsWith("~/"))
        {
            list.Add("~");
            path = path[2..];
        }
        else if (path.StartsWith('~'))
        {
            list.Add("~");
            path = path[1..];
        }

        foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            list.Add(part);
        }

        if (list.Count == 0)
        {
            list.Add(path.Length == 0 ? "/" : path);
        }

        return list;
    }

    private static bool IsDrive(string path) =>
        path.Length >= 2 && path[1] == ':' && char.IsAsciiLetter(path[0]);

    private static List<string> Parse(string pathText)
    {
        var chunks = pathText.Split(Gap);
        var parts = new List<string>(chunks.Length);
        for (var i = 0; i < chunks.Length; i++)
        {
            var chunk = chunks[i];
            if (i == 0 && chunk.StartsWith(' '))
            {
                chunk = chunk[1..];
            }

            if (i == chunks.Length - 1 && chunk.EndsWith(' '))
            {
                chunk = chunk[..^1];
            }

            parts.Add(chunk);
        }

        return parts;
    }

    private static List<string> Shrink(List<string> parts, int maxWidth)
    {
        if (parts.Count == 0)
        {
            return ["…"];
        }

        if (Width(parts) <= maxWidth)
        {
            return parts;
        }

        if (parts.Count == 1)
        {
            return [Fit(parts[0], maxWidth - 2)];
        }

        var first = parts[0];
        var last = parts[^1];
        var middle = parts.Count > 2;
        var kept = middle ? new List<string> { first, "…", last } : new List<string> { first, last };
        if (Width(kept) <= maxWidth)
        {
            return kept;
        }

        var head = middle ? new List<string> { first, "…" } : new List<string> { first };
        var room = maxWidth - Width(head) - 3;
        if (room >= 1)
        {
            kept = [.. head, Fit(last, room)];
            if (Width(kept) <= maxWidth)
            {
                return kept;
            }
        }

        if (middle && TryEnds(first, last, withEllipsis: true, maxWidth, out var three))
        {
            return three;
        }

        if (TryEnds(first, last, withEllipsis: false, maxWidth, out var two))
        {
            return two;
        }

        return [Fit(last, maxWidth - 2)];
    }

    private static bool TryEnds(string first, string last, bool withEllipsis, int maxWidth, out List<string> parts)
    {
        var separators = withEllipsis ? 2 : 1;
        var ellipsisWidth = withEllipsis ? 1 : 0;
        var budget = maxWidth - 2 - (separators * 3) - ellipsisWidth;
        if (budget < 2)
        {
            parts = [];
            return false;
        }

        var firstW = Ansi.VisibleWidth(first);
        var lastW = Ansi.VisibleWidth(last);
        int firstBudget;
        int lastBudget;
        if (firstW + lastW <= budget)
        {
            firstBudget = firstW;
            lastBudget = lastW;
        }
        else if (firstW <= budget - 1)
        {
            firstBudget = firstW;
            lastBudget = budget - firstW;
        }
        else
        {
            lastBudget = Math.Max(1, budget / 2);
            firstBudget = budget - lastBudget;
        }

        parts = withEllipsis
            ? [Fit(first, firstBudget), "…", Fit(last, lastBudget)]
            : [Fit(first, firstBudget), Fit(last, lastBudget)];
        return Width(parts) <= maxWidth;
    }

    private static string Fit(string text, int maxWidth)
    {
        if (maxWidth <= 0)
        {
            return string.Empty;
        }

        if (Ansi.VisibleWidth(text) <= maxWidth)
        {
            return text;
        }

        if (maxWidth == 1)
        {
            return "…";
        }

        var sb = new StringBuilder();
        var width = 0;
        foreach (var rune in text.EnumerateRunes())
        {
            var runeWidth = Ansi.VisibleWidth(rune.ToString());
            if (width + runeWidth > maxWidth - 1)
            {
                break;
            }

            sb.Append(rune);
            width += runeWidth;
        }

        sb.Append('…');
        return sb.ToString();
    }

    private static int Width(IReadOnlyList<string> parts) => Ansi.VisibleWidth(Render(parts));

    private static string Render(IReadOnlyList<string> parts)
    {
        var sb = new StringBuilder();
        sb.Append(' ');
        for (var i = 0; i < parts.Count; i++)
        {
            if (i > 0)
            {
                AppendGap(sb);
            }

            sb.Append(parts[i]);
        }

        sb.Append(' ');
        return sb.ToString();
    }

    private static string CreateGap()
    {
        var sb = new StringBuilder();
        AppendGap(sb);
        return sb.ToString();
    }

    private static void AppendGap(StringBuilder sb)
    {
        Ansi.Fg(sb, Palette.Blue);
        sb.Append(' ');
        sb.Append(Ansi.PathSeparator);
        sb.Append(' ');
        Ansi.Fg(sb, Palette.DarkBlue);
    }
}
