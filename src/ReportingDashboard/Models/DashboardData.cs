namespace ReportingDashboard.Models;

public record DashboardData
{
    public HeaderData Header { get; init; } = new();
    public TimelineData Timeline { get; init; } = new();
    public HeatmapData Heatmap { get; init; } = new();
}

public record HeaderData
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "#";
    public string BacklogLabel { get; init; } = "ADO Backlog";
}

public record TimelineData
{
    public string StartDate { get; init; } = "";
    public string EndDate { get; init; } = "";
    public List<MilestoneTrack> Tracks { get; init; } = new();
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";
    public string Label { get; init; } = "";
    public string Color { get; init; } = "#0078D4";
    public List<MilestoneMarker> Markers { get; init; } = new();
}

public record MilestoneMarker
{
    public string Date { get; init; } = "";
    public string Label { get; init; } = "";
    public string Type { get; init; } = "checkpoint";
    public string LabelPosition { get; init; } = "above";
}

public record HeatmapData
{
    public List<TimePeriodColumn> Periods { get; init; } = new();
    public StatusRow Shipped { get; init; } = new();
    public StatusRow InProgress { get; init; } = new();
    public StatusRow Carryover { get; init; } = new();
    public StatusRow Blockers { get; init; } = new();
}

public record TimePeriodColumn
{
    public string Label { get; init; } = "";
    public bool IsCurrent { get; init; } = false;
}

public record StatusRow
{
    public string Emoji { get; init; } = "";
    public List<List<string>> ItemsByPeriod { get; init; } = new();
}

public record DashboardLoadResult
{
    public bool IsSuccess { get; init; }
    public DashboardData? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static DashboardLoadResult Success(DashboardData data) =>
        new() { IsSuccess = true, Data = data };

    public static DashboardLoadResult NotFound(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    public static DashboardLoadResult ParseError(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };
}