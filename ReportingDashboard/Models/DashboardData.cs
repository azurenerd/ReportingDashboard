namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? BacklogUrl { get; set; }
    public string? NowDate { get; set; }
    public TimelineConfig Timeline { get; set; } = new();
    public HeatmapConfig Heatmap { get; set; } = new();
}

public class TimelineConfig
{
    public string StartMonth { get; set; } = "";
    public string EndMonth { get; set; } = "";
    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Color { get; set; } = "#0078D4";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public string Date { get; set; } = "";
    public string Type { get; set; } = "checkpoint";
    public string Label { get; set; } = "";
}

public class HeatmapConfig
{
    public List<string> Months { get; set; } = new();
    public string CurrentMonth { get; set; } = "";
    public List<HeatmapCategory> Categories { get; set; } = new();
}

public class HeatmapCategory
{
    public string Name { get; set; } = "";
    public string CssClass { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}