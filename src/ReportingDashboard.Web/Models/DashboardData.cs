namespace ReportingDashboard.Web.Models;

public record DashboardData
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string BacklogUrl { get; init; } = string.Empty;
    public string TimelineStart { get; init; } = string.Empty;
    public string TimelineEnd { get; init; } = string.Empty;
    public string CurrentDate { get; init; } = string.Empty;
    public string CurrentMonth { get; init; } = string.Empty;
    public List<TimelineTrack> Tracks { get; init; } = new();
    public List<string> Months { get; init; } = new();
    public List<HeatmapCategory> Categories { get; init; } = new();
    public LegendConfig? Legend { get; init; }
}

public record TimelineTrack
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public List<MilestoneMarker> Markers { get; init; } = new();
}

public record MilestoneMarker
{
    public string Date { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? LabelPosition { get; init; }
}

public record HeatmapCategory
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public Dictionary<string, List<string>> ItemsByMonth { get; init; } = new();
}

public record LegendConfig
{
    public List<LegendItem> Items { get; init; } = new();
}

public record LegendItem
{
    public string Label { get; init; } = string.Empty;
    public string MarkerType { get; init; } = string.Empty;
}

public record DashboardDataResult
{
    public bool Success { get; init; }
    public DashboardData? Data { get; init; }
    public string? ErrorMessage { get; init; }
}