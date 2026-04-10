namespace AgentSquad.ReportingDashboard.Models;

public class Milestone
{
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public DateTime DueDate { get; set; }
	public bool IsCompleted { get; set; }
}