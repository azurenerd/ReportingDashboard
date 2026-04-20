namespace ReportingDashboard.Web.Models;

// Stub - T2 will finalize.
public sealed class Milestone
{
    public required DateOnly Date { get; init; }
    public required MilestoneType Type { get; init; }
    public required string Label { get; init; }
    public CaptionPosition? CaptionPosition { get; init; }
}