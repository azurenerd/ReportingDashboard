namespace ReportingDashboard.Web.Models;

public sealed class HeatmapRow
{
    public required HeatmapCategory Category { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Cells { get; init; }
}
