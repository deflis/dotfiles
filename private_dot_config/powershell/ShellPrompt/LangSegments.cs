using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace ShellPrompt;

/// <summary>
/// zsh の _dp_w_langs / _dp_ver 相当: mise current を1回だけ読み、
/// 無ければ各 CLI。どちらも取れなければその言語セグメントは出さない。
/// </summary>
internal static class LangSegments
{
    public static List<Segment> Build(string cwd)
    {
        var mise = new MiseVersions(cwd);
        var result = new List<Segment>();

        if (HasNodeMarkers(cwd) && TryVersion(mise, "node", cwd, out var nodeVer, "node", "--version"))
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

        if ((HasMarkers(cwd, ".perl-version") || HasExtension(cwd, ".pl", ".pm", ".t"))
            && TryVersion(mise, "perl", cwd, out var perlVer, "perl", "-e", "print substr($^V, 1)"))
        {
            result.Add(new Segment(Ansi.PowerlineLeft, Palette.PerlBg, Palette.PerlFg, $"\ue769 {perlVer}"));
        }

        if ((HasMarkers(cwd, "go.mod", "go.work") || HasExtension(cwd, ".go"))
            && TryVersion(mise, "go", cwd, out var goVer, "go", "env", "GOVERSION"))
        {
            if (goVer.StartsWith("go", StringComparison.Ordinal))
            {
                goVer = goVer[2..];
            }

            result.Add(new Segment(Ansi.PowerlineLeft, Palette.GoBg, Palette.GoFg, $"\ue724 {goVer}"));
        }

        if ((HasMarkers(cwd, "Cargo.toml", "rust-toolchain", "rust-toolchain.toml") || HasExtension(cwd, ".rs"))
            && TryVersion(mise, "rust", cwd, out var rustVer, "rustc", "--version"))
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

        if ((HasMarkers(cwd, "Gemfile", "Rakefile") || HasExtension(cwd, ".rb"))
            && TryVersion(mise, "ruby", cwd, out var rubyVer, "ruby", "-e", "print RUBY_VERSION"))
        {
            result.Add(new Segment(Ansi.PowerlineLeft, Palette.RubyBg, Palette.RubyFg, $"\ue791 {rubyVer}"));
        }

        return result;
    }

    private static bool TryVersion(
        MiseVersions mise,
        string tool,
        string cwd,
        out string version,
        string fileName,
        params string[] args)
    {
        if (mise.TryGet(tool, out version))
        {
            return true;
        }

        version = Run(cwd, fileName, args);
        return !string.IsNullOrWhiteSpace(version);
    }

    private static string? DetectNodePackageManager(string cwd)
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

    private static bool HasNodeMarkers(string cwd) =>
        HasMarkers(cwd, "package.json", ".nvmrc", ".node-version")
        || HasExtension(cwd, ".js", ".mjs", ".cjs", ".ts", ".mts", ".cts", ".jsx", ".tsx");

    private static bool HasMarkers(string cwd, params string[] names)
    {
        foreach (var name in names)
        {
            if (File.Exists(Path.Combine(cwd, name)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasExtension(string cwd, params string[] exts)
    {
        foreach (var file in Directory.EnumerateFiles(cwd))
        {
            var ext = Path.GetExtension(file);
            foreach (var want in exts)
            {
                if (ext.Equals(want, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static string Run(string cwd, string fileName, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = cwd,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        try
        {
            using var proc = Process.Start(psi);
            if (proc is null)
            {
                return string.Empty;
            }

            var stdout = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit();
            return proc.ExitCode == 0 ? stdout : string.Empty;
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or Win32Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>zsh の _dp_mv / _dp_mloaded: 初回アクセスで mise current を一括ロード。</summary>
    private sealed class MiseVersions(string cwd)
    {
        private Dictionary<string, string>? _map;

        public bool TryGet(string tool, out string version)
        {
            EnsureLoaded();
            if (_map!.TryGetValue(tool, out var v) && !string.IsNullOrWhiteSpace(v))
            {
                version = v;
                return true;
            }

            version = string.Empty;
            return false;
        }

        private void EnsureLoaded()
        {
            if (_map is not null)
            {
                return;
            }

            _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var psi = new ProcessStartInfo
            {
                FileName = "mise",
                ArgumentList = { "current" },
                WorkingDirectory = cwd,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            try
            {
                using var proc = Process.Start(psi);
                if (proc is null)
                {
                    return;
                }

                var output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    return;
                }

                foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var sp = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (sp.Length >= 2)
                    {
                        _map[sp[0]] = sp[1];
                    }
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException or InvalidOperationException or Win32Exception)
            {
                // mise が無い場合は CLI 側へ
            }
        }
    }
}
