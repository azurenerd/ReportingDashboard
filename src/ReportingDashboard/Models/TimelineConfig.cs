namespace ReportingDashboard.Models;

public class TimelineConfig
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<Track> Tracks { get; set; } = new();
}

public class Track
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = "#0078D4";
    public List<Milestone> Milestones { get; set; } = new();
}

public class Milestone
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = "checkpoint";
    public string Label { get; set; } = string.Empty;
}