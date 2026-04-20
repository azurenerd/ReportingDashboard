namespace ReportingDashboard.Web.Models;

/// <summary>
/// Monthly execution heatmap: 4 category rows (Shipped / In Progress /
/// Carryover / Blockers) across N month columns (v1 default N=4).
/// </summary>
public sealed class Heatmap
{
    /// <summary>Month column headers (e.g., <c>["Jan","Feb","Mar","Apr"]</c>).</summary>
    public required IReadOnlyList<string> Months { get; init; }

    /// <summary>
    /// Zero-based index of the "current" (highlighted) month column, or null to
    /// auto-compute from <c>DateTime.Today</c>.
    /// </summary>
    public int? CurrentMonthIndex { get; init; }

    /// <summary>Maximum items rendered per cell before truncation to "+K more". Default is 4.</summary>
    public int MaxItemsPerCell { get; init; } = 4;

    /// <summary>Exactly four rows, one per <see cref="HeatmapCategory"/>.</summary>
    public required IReadOnlyList<HeatmapRow> Rows { get; init; }
}