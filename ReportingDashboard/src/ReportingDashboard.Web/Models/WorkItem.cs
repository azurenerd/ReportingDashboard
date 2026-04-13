namespace ReportingDashboard.Web.Models;

public class WorkItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? MilestoneId { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? Notes { get; set; }
}