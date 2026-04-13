namespace ReportingDashboard.Web.Models;

public class WorkItem
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Shipped, InProgress, CarriedOver
    public string? MilestoneId { get; set; }
    public string Owner { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // High, Medium, Low
}