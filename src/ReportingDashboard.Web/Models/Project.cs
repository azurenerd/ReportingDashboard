namespace ReportingDashboard.Web.Models;

/// <summary>
/// Project-level header metadata rendered in the top band of the dashboard.
/// </summary>
public sealed class Project
{
    /// <summary>Project title displayed as the 24px bold <c>&lt;h1&gt;</c>.</summary>
    public required string Title { get; init; }

    /// <summary>Subtitle in the format "<c>Org &#x2022; Workstream &#x2022; Month Year</c>".</summary>
    public required string Subtitle { get; init; }

    /// <summary>Optional absolute URL to the ADO backlog. When null or invalid, the link renders as plain text.</summary>
    public string? BacklogUrl { get; init; }

    /// <summary>Visible text for the inline backlog link. Defaults to "&#x2192; ADO Backlog".</summary>
    public string BacklogLinkText { get; init; } = "\u2192 ADO Backlog";

    /// <summary>
    /// Placeholder project used by <c>Dashboard.razor</c> when <c>data.json</c>
    /// fails to load, so the page still renders at 1920&#215;1080 alongside the
    /// <c>ErrorBanner</c>.
    /// </summary>
    public static Project Placeholder { get; } = new()
    {
        Title = "(data.json error)",
        Subtitle = "see error banner above"
    };
}