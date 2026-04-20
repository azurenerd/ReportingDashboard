namespace ReportingDashboard.Web.Models;

// Stub - T2 will finalize.
public sealed class HeatmapRow
{
    public required HeatmapCategory Category { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Cells { get; init; }
}