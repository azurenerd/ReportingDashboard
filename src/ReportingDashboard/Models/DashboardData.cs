using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    public ProjectInfo Project { get; set; } = new();
    public TimelineData Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}

public class ProjectInfo
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? BacklogUrl { get; set; }
    public string CurrentDate { get; set; } = "";
}

public class TimelineData
{
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public string Date { get; set; } = "";
    public string Label { get; set; } = "";
    public string Type { get; set; } = "";
}

public class HeatmapData
{
    public List<string> Months { get; set; } = new();
    public string HighlightMonth { get; set; } = "";
    public List<HeatmapRow> Rows { get; set; } = new();
}

public class HeatmapRow
{
    public string Category { get; set; } = "";
    public string Emoji { get; set; } = "";
    public List<List<string>> Items { get; set; } = new();
}
