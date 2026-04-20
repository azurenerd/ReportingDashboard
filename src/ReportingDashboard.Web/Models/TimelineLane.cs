namespace ReportingDashboard.Web.Models;

// Stub - T2 will finalize.
public sealed class TimelineLane
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Color { get; init; }
    public required IReadOnlyList<Milestone> Milestones { get; init; }
}