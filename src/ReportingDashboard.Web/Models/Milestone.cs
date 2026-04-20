namespace ReportingDashboard.Web.Models;

public enum MilestoneKind
{
    Checkpoint,
    PoC,
    Production
}

public sealed record Milestone(
    DateOnly Date,
    MilestoneKind Kind,
    string Label);