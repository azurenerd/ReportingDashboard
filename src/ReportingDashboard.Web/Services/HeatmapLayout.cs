using System.Globalization;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Pure helper that projects a <see cref="Heatmap"/> model plus "today" into a
/// render-ready <see cref="HeatmapViewModel"/>. Stateless and deterministic;
/// distinct from <see cref="TimelineMath"/> but reuses its generic helpers
/// (<see cref="TimelineMath.TruncateItems"/>, <see cref="TimelineMath.CurrentMonthIndex"/>).
/// </summary>
public static class HeatmapLayout
{
    public const int DefaultMaxItems = 4;

    private static readonly HeatmapCategory[] RowOrder =
    {
        HeatmapCategory.Shipped,
        HeatmapCategory.InProgress,
        HeatmapCategory.Carryover,
        HeatmapCategory.Blockers
    };

    /// <summary>
    /// Builds a heatmap view model from raw data, enforcing the fixed row order
    /// (Shipped / InProgress / Carryover / Blockers), resolving the current
    /// month column, and truncating overflowing cells.
    /// </summary>
    public static HeatmapViewModel Build(
        Heatmap heatmap,
        DateOnly today,
        int defaultMaxItems = DefaultMaxItems)
    {
        ArgumentNullException.ThrowIfNull(heatmap);
        if (defaultMaxItems < 1)
            throw new ArgumentOutOfRangeException(nameof(defaultMaxItems), "must be >= 1");

        var months = heatmap.Months ?? Array.Empty<string>();
        var monthCount = months.Count;

        var currentIndex = ResolveCurrentMonthIndex(heatmap, today, months);
        var maxItems = heatmap.MaxItemsPerCell > 0 ? heatmap.MaxItemsPerCell : defaultMaxItems;

        var byCategory = new Dictionary<HeatmapCategory, HeatmapRow>();
        foreach (var row in heatmap.Rows ?? Array.Empty<HeatmapRow>())
        {
            byCategory[row.Category] = row;
        }

        var rows = new List<HeatmapRowView>(RowOrder.Length);
        foreach (var cat in RowOrder)
        {
            byCategory.TryGetValue(cat, out var row);
            rows.Add(BuildRow(cat, row, monthCount, maxItems));
        }

        return new HeatmapViewModel(months, currentIndex, rows);
    }

    private static int ResolveCurrentMonthIndex(
        Heatmap heatmap, DateOnly today, IReadOnlyList<string> months)
    {
        if (heatmap.CurrentMonthIndex is { } explicitIndex)
        {
            if (explicitIndex >= 0 && explicitIndex < months.Count)
                return explicitIndex;
            return -1;
        }
        return TimelineMath.CurrentMonthIndex(today, months);
    }

    private static HeatmapRowView BuildRow(
        HeatmapCategory category, HeatmapRow? row, int monthCount, int maxItems)
    {
        var cells = new List<HeatmapCellView>(monthCount);
        var sourceCells = row?.Cells;
        for (var i = 0; i < monthCount; i++)
        {
            IReadOnlyList<string> items = sourceCells is not null && i < sourceCells.Count
                ? sourceCells[i] ?? Array.Empty<string>()
                : Array.Empty<string>();
            cells.Add(BuildCell(items, maxItems));
        }
        return new HeatmapRowView(category, HeaderLabel(category), cells);
    }

    private static HeatmapCellView BuildCell(IReadOnlyList<string> items, int maxItems)
    {
        if (items.Count == 0)
        {
            return new HeatmapCellView(Array.Empty<string>(), 0, IsEmpty: true);
        }
        var (kept, overflow) = TimelineMath.TruncateItems(items, maxItems);
        return new HeatmapCellView(kept, overflow, IsEmpty: false);
    }

    /// <summary>Uppercase display label for the category row header.</summary>
    public static string HeaderLabel(HeatmapCategory category) => category switch
    {
        HeatmapCategory.Shipped => "Shipped",
        HeatmapCategory.InProgress => "In Progress",
        HeatmapCategory.Carryover => "Carryover",
        HeatmapCategory.Blockers => "Blockers",
        _ => category.ToString()
    };

    /// <summary>Formats the trailing "+K more" overflow label.</summary>
    public static string FormatOverflow(int count) =>
        "+" + count.ToString(CultureInfo.InvariantCulture) + " more";
}
