namespace ReportingDashboard.Models;

public record DashboardData
{
    public ProjectInfo Project { get; init; } = new();
    public TimelineConfig Timeline { get; init; } = new();
    public List<MilestoneTrack> Tracks { get; init; } = [];
    public HeatmapData Heatmap { get; init; } = new();
}

public record ProjectInfo
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogUrl { get; init; } = "";
    public string CurrentMonth { get; init; } = "";
}

public record TimelineConfig
{
    public List<string> Months { get; init; } = [];
    public double NowPosition { get; init; }
}

public record MilestoneTrack
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Color { get; init; } = "#000";
    public List<Milestone> Milestones { get; init; } = [];
}

public record Milestone
{
    public string Date { get; init; } = "";
    public string Label { get; init; } = "";
    public string Type { get; init; } = "checkpoint";
    public double Position { get; init; }
}

public record HeatmapData
{
    public List<string> Months { get; init; } = [];
    public List<HeatmapCategory> Categories { get; init; } = [];
}

public record HeatmapCategory
{
    public string Name { get; init; } = "";
    public Dictionary<string, List<string>> Items { get; init; } = new();
}