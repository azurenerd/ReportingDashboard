namespace ReportingDashboard.Web.Models;

/// <summary>
/// A single heatmap row: one <see cref="HeatmapCategory"/> across all month columns.
/// </summary>
public sealed class HeatmapRow
{
    /// <summary>Category the row represents (Shipped / InProgress / Carryover / Blockers).</summary>
    public required HeatmapCategory Category { get; init; }

    /// <summary>
    /// Per-column cell contents. Outer length equals <c>Heatmap.Months.Length</c>;
    /// inner list is the free-form item strings for that month/category.
    /// </summary>
    public required IReadOnlyList<IReadOnlyList<string>> Cells { get; init; }
}