using System.Text;

namespace ShellPrompt;

internal static class GitSegment
{
    public static Segment? TryBuild(string cwd)
    {
        var output = RunGit(cwd, "status", "--porcelain=v2", "--branch", "--show-stash");
        if (output is null)
        {
            return null;
        }

        string? oid = null;
        string? head = null;
        string? upstream = null;
        var hasAb = false;
        var ahead = 0;
        var behind = 0;
        var stash = 0;
        var wq = 0;
        var wa = 0;
        var wm = 0;
        var wd = 0;
        var wx = 0;
        var sa = 0;
        var sm = 0;
        var sd = 0;
        var sr = 0;

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith("# branch.oid ", StringComparison.Ordinal))
            {
                oid = line["# branch.oid ".Length..];
            }
            else if (line.StartsWith("# branch.head ", StringComparison.Ordinal))
            {
                head = line["# branch.head ".Length..];
            }
            else if (line.StartsWith("# branch.upstream ", StringComparison.Ordinal))
            {
                upstream = line["# branch.upstream ".Length..];
            }
            else if (line.StartsWith("# branch.ab ", StringComparison.Ordinal))
            {
                var ab = line["# branch.ab ".Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (ab.Length < 2
                    || !int.TryParse(ab[0].TrimStart('+'), out ahead)
                    || !int.TryParse(ab[1].TrimStart('-'), out behind))
                {
                    continue;
                }

                hasAb = true;
            }
            else if (line.StartsWith("# stash ", StringComparison.Ordinal))
            {
                if (!int.TryParse(line["# stash ".Length..], out stash))
                {
                    stash = 0;
                }
            }
            else if (line.Length >= 4 && line[0] is '1' or '2' && line[1] == ' ')
            {
                CountStaged(line[2], ref sa, ref sm, ref sd, ref sr);
                CountWorking(line[3], ref wa, ref wm, ref wd);
            }
            else if (line.StartsWith("u ", StringComparison.Ordinal))
            {
                wx++;
            }
            else if (line.StartsWith("? ", StringComparison.Ordinal))
            {
                wq++;
            }
        }

        var remote = "origin";
        if (!string.IsNullOrEmpty(upstream))
        {
            var slash = upstream.IndexOf('/');
            remote = slash >= 0 ? upstream[..slash] : upstream;
        }

        var url = RunGit(cwd, "config", "--get", $"remote.{remote}.url") ?? string.Empty;
        var icon = UpstreamIcon(url);

        var text = new StringBuilder(icon);
        if (head == "(detached)")
        {
            text.Append("\uf417 ");
            text.Append(oid is { Length: >= 7 } ? oid[..7] : oid);
        }
        else
        {
            text.Append("\ue725 ");
            text.Append(head);
        }

        if (!string.IsNullOrEmpty(upstream))
        {
            if (!hasAb)
            {
                text.Append('≢');
            }
            else if (ahead == 0 && behind == 0)
            {
                text.Append('≡');
            }
            else
            {
                if (ahead > 0)
                {
                    text.Append('↑').Append(ahead);
                }

                if (ahead > 0 && behind > 0)
                {
                    text.Append(' ');
                }

                if (behind > 0)
                {
                    text.Append('↓').Append(behind);
                }
            }
        }

        var wstr = FormatChanges(wq, wa, wm, wd, 0, wx);
        var sstr = FormatChanges(0, sa, sm, sd, sr, 0);
        if (wstr.Length > 0)
        {
            text.Append(" \uf044 ").Append(wstr);
        }

        if (wstr.Length > 0 && sstr.Length > 0)
        {
            text.Append(" |");
        }

        if (sstr.Length > 0)
        {
            text.Append(" \uf046 ").Append(sstr);
        }

        if (stash > 0)
        {
            text.Append(" \uf692 ").Append(stash);
        }

        var workingChanged = wstr.Length > 0;
        var stagingChanged = sstr.Length > 0;
        Rgb bg;
        Rgb fg;
        if (workingChanged || stagingChanged)
        {
            bg = Palette.Lime;
            fg = Palette.Black;
        }
        else if (ahead > 0 && behind > 0)
        {
            bg = Palette.Magenta;
            fg = Palette.White;
        }
        else if (ahead > 0)
        {
            bg = Palette.YellowOrange;
            fg = Palette.Black;
        }
        else if (behind > 0)
        {
            bg = Palette.Cyan;
            fg = Palette.White;
        }
        else
        {
            bg = Palette.Green;
            fg = Palette.Black;
        }

        return new Segment(Ansi.PowerlineLeftRound, bg, fg, text.ToString());
    }

    private static void CountStaged(char c, ref int a, ref int m, ref int d, ref int r)
    {
        switch (c)
        {
            case 'A':
                a++;
                break;
            case 'M':
            case 'T':
                m++;
                break;
            case 'D':
                d++;
                break;
            case 'R':
            case 'C':
                r++;
                break;
        }
    }

    private static void CountWorking(char c, ref int a, ref int m, ref int d)
    {
        switch (c)
        {
            case 'A':
                a++;
                break;
            case 'M':
            case 'T':
                m++;
                break;
            case 'D':
                d++;
                break;
        }
    }

    private static string FormatChanges(int q, int a, int m, int d, int r, int x)
    {
        var parts = new List<string>();
        if (q > 0) parts.Add($"?{q}");
        if (a > 0) parts.Add($"+{a}");
        if (m > 0) parts.Add($"~{m}");
        if (d > 0) parts.Add($"-{d}");
        if (r > 0) parts.Add($">{r}");
        if (x > 0) parts.Add($"x{x}");
        return string.Join(' ', parts);
    }

    private static string UpstreamIcon(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        if (url.Contains("github", StringComparison.OrdinalIgnoreCase))
        {
            return "\uf408";
        }

        if (url.Contains("gitlab", StringComparison.OrdinalIgnoreCase))
        {
            return "\uf296";
        }

        if (url.Contains("bitbucket", StringComparison.OrdinalIgnoreCase))
        {
            return "\uf171";
        }

        if (url.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase)
            || url.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase))
        {
            return "\uebe8";
        }

        return "\ue5fb";
    }

    private static string? RunGit(string cwd, params string[] args) =>
        ProcessRunner.Run(cwd, "git", args, new Dictionary<string, string> { ["GIT_OPTIONAL_LOCKS"] = "0" });
}
