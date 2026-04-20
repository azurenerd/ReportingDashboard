namespace ReportingDashboard.Web.Models;

// Stub - T2 will finalize.
public sealed class Heatmap
{
    public required IReadOnlyList<string> Months { get; init; }
    public int? CurrentMonthIndex { get; init; }
    public int MaxItemsPerCell { get; init; } = 4;
    public required IReadOnlyList<HeatmapRow> Rows { get; init; }
}