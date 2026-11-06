using System.Text;

namespace ShellPrompt;

/// <summary>
/// zsh の _dp_w_langs / _dp_ver 相当: mise current を1回だけ読み、
/// 無ければ各 CLI。どちらも取れなければその言語セグメントは出さない。
/// </summary>
internal static class LangSegments
{
    public static List<Segment> Build(string cwd, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var mise = new MiseVersions(cwd, cancellationToken);
        var result = new List<Segment>();

        if (HasNodeMarkers(cwd, cancellationToken) && TryVersion(mise, "node", cwd, cancellationToken, out var nodeVer, "node", "--version"))
        {
            var text = new StringBuilder("\ue718 ");
            var pm = DetectNodePackageManager(cwd);
            if (pm is not null)
            {
                text.Append(pm);
            }

            text.Append(nodeVer.TrimStart('v'));
            result.Add(new Segment(Ansi.PowerlineLeft, Palette.NodeBg, Palette.NodeFg, text.ToString()));
        }

        if ((HasMarkers(cwd, cancellationToken, ".perl-version") || HasExtension(cwd, cancellationToken, ".pl", ".pm"))
            && TryVersion(mise, "perl", cwd, cancellationToken, out var perlVer, "perl", "-e", "print substr($^V, 1)"))
        {
            result.Add(new Segment(Ansi.PowerlineLeft, Palette.PerlBg, Palette.PerlFg, $"\ue769 {perlVer}"));
        }

        if ((HasMarkers(cwd, cancellationToken, "go.mod", "go.work") || HasExtension(cwd, cancellationToken, ".go"))
            && TryVersion(mise, "go", cwd, cancellationToken, out var goVer, "go", "env", "GOVERSION"))
        {
            if (goVer.StartsWith("go", StringComparison.Ordinal))
            {
                goVer = goVer[2..];
            }

            result.Add(new Segment(Ansi.PowerlineLeft, Palette.GoBg, Palette.GoFg, $"\ufcd1 {goVer}"));
        }

        if ((HasMarkers(cwd, cancellationToken, "Cargo.toml", "rust-toolchain", "rust-toolchain.toml") || HasExtension(cwd, cancellationToken, ".rs"))
            && TryVersion(mise, "rust", cwd, cancellationToken, out var rustVer, "rustc", "--version"))
        {
            if (rustVer.StartsWith("rustc ", StringComparison.Ordinal))
            {
                rustVer = rustVer["rustc ".Length..];
            }

            var space = rustVer.IndexOf(' ');
            if (space >= 0)
            {
                rustVer = rustVer[..space];
            }

            result.Add(new Segment(Ansi.PowerlineLeft, Palette.RustBg, Palette.RustFg, $"\ue7a8 {rustVer}"));
        }

        if ((HasMarkers(cwd, cancellationToken, "Gemfile", "Rakefile") || HasExtension(cwd, cancellationToken, ".rb"))
            && TryVersion(mise, "ruby", cwd, cancellationToken, out var rubyVer, "ruby", "-e", "print RUBY_VERSION"))
        {
            result.Add(new Segment(Ansi.PowerlineLeft, Palette.RubyBg, Palette.RubyFg, $"\ue791 {rubyVer}"));
        }

        return result;
    }

    private static bool TryVersion(
        MiseVersions mise,
        string tool,
        string cwd,
        CancellationToken cancellationToken,
        out string version,
        string fileName,
        params string[] args)
    {
        if (mise.TryGet(tool, out version))
        {
            return true;
        }

        version = Run(cwd, fileName, cancellationToken, args);
        return !string.IsNullOrWhiteSpace(version);
    }

    private static string? DetectNodePackageManager(string cwd)
    {
        try
        {
            return DetectNodePackageManagerCore(cwd);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static string? DetectNodePackageManagerCore(string cwd)
    {
        if (File.Exists(Path.Combine(cwd, "yarn.lock")))
        {
            return Fg(Palette.YarnIcon) + "\U000F011B" + Fg(Palette.NodeFg) + " ";
        }

        if (File.Exists(Path.Combine(cwd, "package-lock.json")))
        {
            return Fg(Palette.NpmIcon) + "\ue71e" + Fg(Palette.NodeFg) + " ";
        }

        if (File.Exists(Path.Combine(cwd, "pnpm-lock.yaml")))
        {
            return "\U000F02C1 ";
        }

        if (File.Exists(Path.Combine(cwd, "bun.lock")) || File.Exists(Path.Combine(cwd, "bun.lockb")))
        {
            return "\ue76f ";
        }

        return null;
    }

    private static string Fg(Rgb c) => $"\x1b[38;2;{c.R};{c.G};{c.B}m";

    private static bool HasNodeMarkers(string cwd, CancellationToken cancellationToken) =>
        HasMarkers(cwd, cancellationToken, "package.json", ".nvmrc", ".node-version")
        || HasExtension(cwd, cancellationToken, ".js", ".mjs", ".cjs", ".ts", ".mts", ".cts", ".jsx", ".tsx");

    private static bool HasMarkers(string start, CancellationToken cancellationToken, params string[] names)
    {
        for (var dir = start; dir is not null; dir = ParentDirectory(dir))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var name in names)
            {
                try
                {
                    if (File.Exists(Path.Combine(dir, name)))
                    {
                        return true;
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // 読めないディレクトリは飛ばして親を見る。
                }
            }
        }

        return false;
    }

    private static bool HasExtension(string start, CancellationToken cancellationToken, params string[] exts)
    {
        const int maxFiles = 200;
        for (var dir = start; dir is not null; dir = ParentDirectory(dir))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var n = 0;
                foreach (var file in Directory.EnumerateFiles(dir))
                {
                    if (++n > maxFiles)
                    {
                        break;
                    }

                    var ext = Path.GetExtension(file);
                    foreach (var want in exts)
                    {
                        if (ext.Equals(want, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // ディレクトリが無い、または列挙できない。親へ続ける。
            }
        }

        return false;
    }

    private static string? ParentDirectory(string dir)
    {
        string? parent;
        try
        {
            parent = Directory.GetParent(dir)?.FullName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return null;
        }

        if (parent is null || string.Equals(parent, dir, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return parent;
    }

    private static string Run(string cwd, string fileName, CancellationToken cancellationToken, params string[] args)
    {
        var result = ProcessRunner.Run(cwd, fileName, args, cancellationToken);
        if (result is null || result.Value.ExitCode != 0)
        {
            return string.Empty;
        }

        return result.Value.Stdout.Trim();
    }

    /// <summary>zsh の _dp_mv / _dp_mloaded: 初回アクセスで mise current を一括ロード。</summary>
    private sealed class MiseVersions(string cwd, CancellationToken cancellationToken)
    {
        private Dictionary<string, string>? _map;

        public bool TryGet(string tool, out string version)
        {
            EnsureLoaded();
            if (_map!.TryGetValue(tool, out var value) && !IsFallback(value))
            {
                version = value;
                return true;
            }

            version = string.Empty;
            return false;
        }

        private static bool IsFallback(string value) =>
            string.IsNullOrWhiteSpace(value)
            || value.Equals("system", StringComparison.OrdinalIgnoreCase)
            || value.Equals("path", StringComparison.OrdinalIgnoreCase);

        private void EnsureLoaded()
        {
            if (_map is not null)
            {
                return;
            }

            _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var result = ProcessRunner.Run(cwd, "mise", ["current"], cancellationToken);
            if (result is null || result.Value.ExitCode != 0)
            {
                return;
            }

            foreach (var line in result.Value.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var sp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (sp.Length >= 2)
                {
                    _map[sp[0]] = sp[1];
                }
            }
        }
    }
}
