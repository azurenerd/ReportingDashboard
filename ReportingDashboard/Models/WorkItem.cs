namespace ReportingDashboard.Models;

public class WorkItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // shipped, in-progress, carried-over, blocked
    public string Owner { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // high, medium, low
    public string Month { get; set; } = string.Empty;
    public string? Notes { get; set; }
}