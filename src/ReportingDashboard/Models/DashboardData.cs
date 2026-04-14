namespace ReportingDashboard.Models;

public record DashboardData
{
    public required int SchemaVersion { get; init; }
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogUrl { get; init; }
    public required TimelineConfig Timeline { get; init; }
    public required HeatmapConfig Heatmap { get; init; }
    public string? NowDateOverride { get; init; }
    public string? CurrentMonthOverride { get; init; }
}

public record TimelineConfig
{
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public required Workstream[] Workstreams { get; init; }
}

public record Workstream
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
    public required Milestone[] Milestones { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }
    public required string Date { get; init; }
    public required string Type { get; init; }
    public string? LabelPosition { get; init; }
}

public record HeatmapConfig
{
    public required string[] MonthColumns { get; init; }
    public required StatusCategory[] Categories { get; init; }
}

public record StatusCategory
{
    public required string Name { get; init; }
    public required string Emoji { get; init; }
    public required string CssClass { get; init; }
    public required MonthItems[] Months { get; init; }
}

public record MonthItems
{
    public required string Month { get; init; }
    public required string[] Items { get; init; }
}