namespace ReportingDashboard.Web.Models;

public sealed record Milestone
{
    public DateOnly Date { get; init; }
    public MilestoneType Type { get; init; }
    public string Label { get; init; } = string.Empty;
    public LabelPosition? LabelPosition { get; init; }
}