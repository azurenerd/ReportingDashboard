namespace ReportingDashboard.Web.Models;

public class DashboardData
{
    public ProjectInfo ProjectInfo { get; set; } = new();
    public List<Milestone> Milestones { get; set; } = new();
    public List<WorkItem> WorkItems { get; set; } = new();
}