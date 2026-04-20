namespace ReportingDashboard.Web.Models;

// TODO(T2): add required init properties, BacklogLinkText default, validator-friendly shape.
public sealed class Project
{
    public string? Title { get; init; }
    public string? Subtitle { get; init; }
    public string? BacklogUrl { get; init; }
    public string BacklogLinkText { get; init; } = "\u2192 ADO Backlog";

    public static Project Placeholder { get; } = new()
    {
        Title = "(placeholder)",
        Subtitle = ""
    };
}