namespace AgentSquad.Runner.Data;

public record DashboardReport
{
    public HeaderInfo Header { get; init; } = new();
    public List<TimelineTrack> TimelineTracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; } = new();
}

public record HeaderInfo
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "#";
    public string ReportDate { get; init; } = "";
    public string TimelineStartDate { get; init; } = "";
    public string TimelineEndDate { get; init; } = "";
    public List<string> TimelineMonths { get; init; } = [];
}

public record TimelineTrack
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "#999";
    public List<Milestone> Milestones { get; init; } = [];
}

public record Milestone
{
    public string Label { get; init; } = "";
    public string Date { get; init; } = "";
    public string Type { get; init; } = "checkpoint";
    public string? LabelPosition { get; init; }
}

public record HeatmapData
{
    public List<string> Columns { get; init; } = [];
    public int HighlightColumnIndex { get; init; }
    public List<HeatmapRow> Rows { get; init; } = [];
}

public record HeatmapRow
{
    public string Category { get; init; } = "";
    public string Label { get; init; } = "";
    public List<List<string>> CellItems { get; init; } = [];
}