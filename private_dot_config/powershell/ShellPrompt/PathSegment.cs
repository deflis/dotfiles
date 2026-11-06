using System.Text;

namespace ShellPrompt;

internal static class PathSegment
{
    public static string Format(string cwd, string homeDir)
    {
        var path = cwd;
        if (!string.IsNullOrEmpty(homeDir) && IsHomePrefix(path, homeDir))
        {
            path = "~" + path[homeDir.Length..];
        }

        path = path.Replace('\\', '/');
        var parts = SplitPath(path);
        var sb = new StringBuilder();
        sb.Append(' ');
        for (var i = 0; i < parts.Count; i++)
        {
            if (i > 0)
            {
                Ansi.Fg(sb, Palette.Blue);
                sb.Append(' ');
                sb.Append(Ansi.PathSeparator);
                sb.Append(' ');
                Ansi.Fg(sb, Palette.DarkBlue);
            }

            sb.Append(parts[i]);
        }

        sb.Append(' ');
        return sb.ToString();
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

    private static List<string> SplitPath(string path)
    {
        if (path is "/" or "~")
        {
            return [path];
        }

        var parts = new List<string>();
        if (path.StartsWith('/'))
        {
            parts.Add("/");
            path = path[1..];
        }
        else if (path.StartsWith("~/"))
        {
            parts.Add("~");
            path = path[2..];
        }
        else if (path.StartsWith('~'))
        {
            parts.Add("~");
            path = path[1..];
        }

        foreach (var part in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            parts.Add(part);
        }

        if (parts.Count == 0)
        {
            parts.Add(path);
        }

        return parts;
    }
}
