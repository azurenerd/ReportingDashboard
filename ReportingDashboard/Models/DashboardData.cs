namespace ReportingDashboard.Models;

public class DashboardData
{
    public string ProjectName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string ReportingPeriod { get; set; } = string.Empty;
    public string OverallStatus { get; set; } = string.Empty;
    public string HealthIndicator { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string BacklogUrl { get; set; } = string.Empty;
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> ShippedItems { get; set; } = new();
    public List<WorkItem> InProgressItems { get; set; } = new();
    public List<WorkItem> CarriedOverItems { get; set; } = new();
    public List<WorkItem> BlockedItems { get; set; } = new();
}