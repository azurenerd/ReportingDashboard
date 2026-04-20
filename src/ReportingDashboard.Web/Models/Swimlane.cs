namespace ReportingDashboard.Web.Models;

public sealed record Swimlane
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Color { get; init; } = "#0078D4";
    public IReadOnlyList<Milestone> Milestones { get; init; } = Array.Empty<Milestone>();
}