namespace ReportingDashboard.Models;

public record MonthSummary
{
    public string? Month { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int CarriedItems { get; init; }
    public string OverallHealth { get; init; } = "Unknown";
}