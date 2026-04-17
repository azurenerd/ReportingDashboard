namespace ReportingDashboard.Models;

public record DashboardData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "";
    public string CurrentDate { get; init; } = "";
    public string TimelineStartMonth { get; init; } = "";
    public string TimelineEndMonth { get; init; } = "";
    public List<Milestone> Milestones { get; init; } = new();
    public HeatmapData Heatmap { get; init; } = new();
}

public record Milestone
{
    public string Id { get; init; } = "";
    public string Label { get; init; } = "";
    public string Color { get; init; } = "";
    public List<MilestoneEvent> Events { get; init; } = new();
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";
    public string Type { get; init; } = "";
    public string Label { get; init; } = "";
    public string? LabelPosition { get; init; }
}

public record HeatmapData
{
    public List<string> Months { get; init; } = new();
    public int CurrentMonthIndex { get; init; }
    public List<HeatmapRow> Rows { get; init; } = new();
}

public record HeatmapRow
{
    public string Category { get; init; } = "";
    public Dictionary<string, List<string>> Items { get; init; } = new();
}