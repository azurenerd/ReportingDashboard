namespace ReportingDashboard.Models;

public record DashboardData
{
    public required int SchemaVersion { get; init; }
    public required HeaderInfo Header { get; init; }
    public required TimelineData Timeline { get; init; }
    public required HeatmapData Heatmap { get; init; }
}

public record HeaderInfo
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public required string BacklogLink { get; init; }
    public required string CurrentDate { get; init; }
}

public record TimelineData
{
    public required string StartDate { get; init; }
    public required string EndDate { get; init; }
    public required List<TimelineTrack> Tracks { get; init; }
}

public record TimelineTrack
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required string Color { get; init; }
    public required List<Milestone> Milestones { get; init; }
}

public record Milestone
{
    public required string Label { get; init; }
    public required string Date { get; init; }
    public required string Type { get; init; }
}

public record HeatmapData
{
    public required List<string> Columns { get; init; }
    public string? HighlightColumn { get; init; }
    public required List<HeatmapRow> Rows { get; init; }
}

public record HeatmapRow
{
    public required string Category { get; init; }
    public required string CategoryStyle { get; init; }
    public required List<List<string>> Cells { get; init; }
}