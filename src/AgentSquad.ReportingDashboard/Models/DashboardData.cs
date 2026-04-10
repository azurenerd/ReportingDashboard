namespace AgentSquad.ReportingDashboard.Models;

public class DashboardData
{
	public string ProjectName { get; set; } = string.Empty;
	public string ProjectStatus { get; set; } = string.Empty;
	public List<Milestone> Milestones { get; set; } = new();
	public List<MetricItem> ShippedItems { get; set; } = new();
	public List<MetricItem> InProgressItems { get; set; } = new();
	public List<MetricItem> CarryoverItems { get; set; } = new();
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}