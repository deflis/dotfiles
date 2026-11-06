using System.Management.Automation;

namespace ShellPrompt;

[Cmdlet(VerbsCommon.Get, "ShellPrompt")]
[OutputType(typeof(string))]
public sealed class GetShellPromptCommand : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public int ExitCode { get; set; }

    [Parameter(Mandatory = true)]
    public long ExecutionMs { get; set; }

    [Parameter(Mandatory = true)]
    public string Cwd { get; set; } = null!;

    [Parameter(Mandatory = true)]
    public string ShellName { get; set; } = null!;

    [Parameter(Mandatory = true)]
    public int Columns { get; set; }

    [Parameter(Mandatory = true)]
    public string HomeDir { get; set; } = null!;

    [Parameter(Mandatory = true)]
    public string HostName { get; set; } = null!;

    [Parameter(Mandatory = true)]
    public DateTimeOffset Now { get; set; }

    protected override void EndProcessing()
    {
        if (string.IsNullOrWhiteSpace(Cwd))
        {
            throw new PSArgumentException("Cwd must not be empty.", nameof(Cwd));
        }

        if (string.IsNullOrWhiteSpace(HomeDir))
        {
            throw new PSArgumentException("HomeDir must not be empty.", nameof(HomeDir));
        }

        if (Columns < 1)
        {
            throw new PSArgumentOutOfRangeException(nameof(Columns), Columns, "Columns must be >= 1.");
        }

        var isSsh = MiscSegments.IsSsh();
        var (osText, osBg, osFg) = OsSegment.Get(isSsh, HostName);
        var pathText = PathSegment.Format(Cwd, HomeDir);

        var right = new List<Segment>();
        right.AddRange(AsyncSegmentCache.GetAndRefresh(Cwd));

        var exec = MiscSegments.ExecutionTime(ExecutionMs);
        if (exec is { } e)
        {
            right.Add(e);
        }

        right.Add(MiscSegments.Shell(ShellName));
        right.Add(MiscSegments.Time(Now));

        var rendered = PromptRenderer.Render(
            osText,
            osBg,
            osFg,
            pathText,
            right,
            ExitCode,
            MiscSegments.IsRoot(),
            Columns);

        WriteObject(rendered);
    }
}
