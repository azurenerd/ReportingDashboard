namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string BacklogUrl { get; set; } = "";
    public TimelineData Timeline { get; set; } = new();
    public HeatmapData Heatmap { get; set; } = new();
}

public class TimelineData
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? NowDate { get; set; }
    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#0078D4";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = "";
    public MilestoneType Type { get; set; }
}

public enum MilestoneType
{
    Checkpoint,
    PoC,
    Production,
    Dot
}

public class HeatmapData
{
    public List<string> Months { get; set; } = new();
    public string? CurrentMonth { get; set; }
    public List<StatusCategory> Categories { get; set; } = new();
}

public class StatusCategory
{
    public string Name { get; set; } = "";
    public string CssClass { get; set; } = "";
    public string Icon { get; set; } = "";
    public Dictionary<string, List<string>> Items { get; set; } = new();
}