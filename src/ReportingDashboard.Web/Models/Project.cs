namespace ReportingDashboard.Web.Models;

// Stub - T2 will finalize.
public sealed class Project
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public string? BacklogUrl { get; init; }
    public string BacklogLinkText { get; init; } = "\u2192 ADO Backlog";

    public static Project Placeholder { get; } = new()
    {
        Title = "(data.json error)",
        Subtitle = "see error banner above"
    };
}