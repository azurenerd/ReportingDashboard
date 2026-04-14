namespace ReportingDashboard.Models;

public class DashboardData
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public string CurrentDate { get; set; } = string.Empty;
    public List<string> Months { get; set; } = new();
    public List<TrackModel> Tracks { get; set; } = new();
    public StatusRowsModel StatusRows { get; set; } = new();
}

public class TrackModel
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public List<MilestoneModel> Milestones { get; set; } = new();
}

public class MilestoneModel
{
    public string Date { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class StatusRowsModel
{
    public Dictionary<string, List<string>> Shipped { get; set; } = new();
    public Dictionary<string, List<string>> InProgress { get; set; } = new();
    public Dictionary<string, List<string>> Carryover { get; set; } = new();
    public Dictionary<string, List<string>> Blockers { get; set; } = new();
}