using System.Text;

namespace ShellPrompt;

internal static class GitSegment
{
    private static readonly Dictionary<string, string> GitEnv = new()
    {
        ["GIT_OPTIONAL_LOCKS"] = "0",
    };

    public static Segment? TryBuild(string cwd, CancellationToken cancellationToken)
    {
        var status = RunGit(cwd, cancellationToken, "status", "--porcelain=v2", "--branch", "--show-stash");
        if (status is null)
        {
            return null;
        }

        if (status.Value.ExitCode != 0)
        {
            if (IsNotARepository(status.Value.Stderr))
            {
                return null;
            }

            ThrowGitFailure(status.Value);
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

        foreach (var line in status.Value.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
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

        var detached = head == "(detached)";
        if (detached && TrySymbolicHead(cwd, cancellationToken, out var symbolicHead))
        {
            head = symbolicHead;
            detached = false;
        }

        var remote = "origin";
        if (!string.IsNullOrEmpty(upstream))
        {
            var slash = upstream.IndexOf('/');
            remote = slash >= 0 ? upstream[..slash] : upstream;
        }

        var url = ReadRemoteUrl(cwd, cancellationToken, remote);
        var icon = UpstreamIcon(url);

        var text = new StringBuilder(icon);
        if (detached)
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
        else if (!string.IsNullOrEmpty(upstream) && !hasAb)
        {
            bg = Palette.Magenta;
            fg = Palette.White;
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

    private static string ReadRemoteUrl(string cwd, CancellationToken cancellationToken, string remote)
    {
        var config = RunGit(cwd, cancellationToken, "config", "--get", $"remote.{remote}.url");
        if (config is null)
        {
            throw new InvalidOperationException("git config failed to start.");
        }

        if (config.Value.ExitCode == 1)
        {
            return string.Empty;
        }

        if (config.Value.ExitCode != 0)
        {
            ThrowGitFailure(config.Value);
        }

        return config.Value.Stdout.Trim();
    }

    /// <summary>
    /// porcelain が detached でも symbolic-ref が ref を返せば通常ブランチとして扱う。
    /// -q の終了コード 1 は本物の detached。
    /// </summary>
    private static bool TrySymbolicHead(string cwd, CancellationToken cancellationToken, out string branch)
    {
        var symbolic = RunGit(cwd, cancellationToken, "symbolic-ref", "-q", "HEAD");
        if (symbolic is null)
        {
            throw new InvalidOperationException("git symbolic-ref failed to start.");
        }

        if (symbolic.Value.ExitCode == 1)
        {
            branch = string.Empty;
            return false;
        }

        if (symbolic.Value.ExitCode != 0)
        {
            ThrowGitFailure(symbolic.Value);
        }

        var refName = symbolic.Value.Stdout.Trim();
        const string heads = "refs/heads/";
        if (refName.StartsWith(heads, StringComparison.Ordinal))
        {
            refName = refName[heads.Length..];
        }

        if (refName.Length == 0)
        {
            branch = string.Empty;
            return false;
        }

        branch = refName;
        return true;
    }

    private static bool IsNotARepository(string stderr) =>
        stderr.Contains("not a git repository", StringComparison.OrdinalIgnoreCase);

    private static void ThrowGitFailure(ProcessResult result)
    {
        var stderr = result.Stderr.Trim();
        var message = stderr.Length > 0
            ? stderr
            : $"git exited with code {result.ExitCode}.";
        throw new InvalidOperationException(message);
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

        if (!TryGetGitHost(url, out var host))
        {
            return "\ue5fb";
        }

        if (HostIs(host, "github.com"))
        {
            return "\uf408";
        }

        if (HostIs(host, "gitlab.com"))
        {
            return "\uf296";
        }

        if (HostIs(host, "bitbucket.org"))
        {
            return "\uf171";
        }

        if (HostIs(host, "dev.azure.com") || HostIs(host, "visualstudio.com"))
        {
            return "\uebe8";
        }

        return "\ue5fb";
    }

    private static bool HostIs(string host, string domain) =>
        host.Equals(domain, StringComparison.OrdinalIgnoreCase)
        || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);

    private static bool TryGetGitHost(string url, out string host)
    {
        host = string.Empty;
        var trimmed = url.Trim();
        if (trimmed.Length == 0)
        {
            return false;
        }

        var scheme = trimmed.IndexOf("://", StringComparison.Ordinal);
        if (scheme > 0)
        {
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host))
            {
                host = uri.Host;
                return true;
            }

            return false;
        }

        var colon = trimmed.IndexOf(':');
        if (colon <= 0)
        {
            return false;
        }

        var authority = trimmed[..colon];
        var at = authority.LastIndexOf('@');
        if (at >= 0)
        {
            authority = authority[(at + 1)..];
        }

        if (authority.Length == 0 || authority.Contains('/') || authority.Contains('\\'))
        {
            return false;
        }

        host = authority;
        return true;
    }

    private static ProcessResult? RunGit(string cwd, CancellationToken cancellationToken, params string[] args) =>
        ProcessRunner.Run(cwd, "git", args, cancellationToken, GitEnv);
}
