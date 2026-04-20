namespace ReportingDashboard.Web.Models;

public sealed record MilestoneTrack(
    string Id,
    string Label,
    string Color,
    IReadOnlyList<Milestone> Milestones);