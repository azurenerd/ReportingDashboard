namespace ReportingDashboard.Models;

public record DashboardConfig
{
    public string? ProjectName { get; init; }
    public string? Workstream { get; init; }
    public string? ReportMonth { get; init; }
    public string? BacklogUrl { get; init; }
    public TimelineConfig? Timeline { get; init; }
    public HeatmapConfig? Heatmap { get; init; }
}

public record TimelineConfig
{
    public string? StartDate { get; init; }
    public string? EndDate { get; init; }
    public string? NowDate { get; init; }
    public List<Milestone>? Milestones { get; init; }
}

public record Milestone
{
    public string? Id { get; init; }
    public string? Label { get; init; }
    public string? Color { get; init; }
    public List<TimelineEvent>? Events { get; init; }
}

public record TimelineEvent
{
    public string? Date { get; init; }
    public string? Label { get; init; }
    public string? Type { get; init; }
}

public record HeatmapConfig
{
    public List<string>? Columns { get; init; }
    public string? CurrentColumn { get; init; }
    public Dictionary<string, List<string>>? Shipped { get; init; }
    public Dictionary<string, List<string>>? InProgress { get; init; }
    public Dictionary<string, List<string>>? Carryover { get; init; }
    public Dictionary<string, List<string>>? Blockers { get; init; }
}