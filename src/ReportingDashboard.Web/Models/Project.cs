namespace ReportingDashboard.Web.Models;

// TODO(T2): flesh out with required Title/Subtitle/BacklogUrl/BacklogLinkText init props per architecture.
public sealed class Project
{
    public string Title { get; init; } = "(placeholder)";
    public string Subtitle { get; init; } = string.Empty;
    public string? BacklogUrl { get; init; }
    public string BacklogLinkText { get; init; } = "\u2192 ADO Backlog";

    public static Project Placeholder { get; } = new()
    {
        Title = "(placeholder)",
        Subtitle = string.Empty
    };
}