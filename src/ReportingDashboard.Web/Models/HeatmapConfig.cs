namespace ReportingDashboard.Web.Models;

public sealed record HeatmapConfig
{
    public IReadOnlyList<string> Months { get; init; } = Array.Empty<string>();
    public int CurrentMonthIndex { get; init; }
    public HeatmapRows Rows { get; init; } = new();
}