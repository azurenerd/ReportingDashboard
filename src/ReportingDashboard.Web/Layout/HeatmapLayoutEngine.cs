using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Layout;

public static class HeatmapLayoutEngine
{
    private static readonly HeatmapCategory[] CategoryOrder =
    {
        HeatmapCategory.Shipped,
        HeatmapCategory.InProgress,
        HeatmapCategory.Carryover,
        HeatmapCategory.Blockers
    };

    private static readonly Dictionary<HeatmapCategory, string> HeaderLabels = new()
    {
        [HeatmapCategory.Shipped] = "SHIPPED",
        [HeatmapCategory.InProgress] = "IN PROGRESS",
        [HeatmapCategory.Carryover] = "CARRYOVER",
        [HeatmapCategory.Blockers] = "BLOCKERS"
    };

    public static HeatmapViewModel Build(Heatmap heatmap, DateOnly today, int defaultMaxItems = 4)
    {
        ArgumentNullException.ThrowIfNull(heatmap);

        var months = heatmap.Months;
        var maxItems = heatmap.MaxItemsPerCell > 0 ? heatmap.MaxItemsPerCell : defaultMaxItems;
        var currentIndex = ResolveCurrentMonthIndex(heatmap, today);

        var rowsByCategory = heatmap.Rows.ToDictionary(r => r.Category);
        var rowViews = new List<HeatmapRowView>(CategoryOrder.Length);

        foreach (var category in CategoryOrder)
        {
            var cells = new List<HeatmapCellView>(months.Count);
            if (rowsByCategory.TryGetValue(category, out var row))
            {
                for (var i = 0; i < months.Count; i++)
                {
                    var items = i < row.Cells.Count ? row.Cells[i] : Array.Empty<string>();
                    cells.Add(BuildCell(items, maxItems));
                }
            }
            else
            {
                for (var i = 0; i < months.Count; i++)
                {
                    cells.Add(new HeatmapCellView(Array.Empty<string>(), 0, IsEmpty: true));
                }
            }

            rowViews.Add(new HeatmapRowView(category, HeaderLabels[category], cells));
        }

        return new HeatmapViewModel(months, currentIndex, rowViews);
    }

    private static HeatmapCellView BuildCell(IReadOnlyList<string> items, int maxItems)
    {
        if (items.Count == 0)
        {
            return new HeatmapCellView(Array.Empty<string>(), 0, IsEmpty: true);
        }

        if (items.Count <= maxItems)
        {
            return new HeatmapCellView(items, 0, IsEmpty: false);
        }

        var keep = Math.Max(maxItems - 1, 0);
        var visible = new List<string>(keep);
        for (var i = 0; i < keep; i++)
        {
            visible.Add(items[i]);
        }
        var overflow = items.Count - keep;
        return new HeatmapCellView(visible, overflow, IsEmpty: false);
    }

    private static int ResolveCurrentMonthIndex(Heatmap heatmap, DateOnly today)
    {
        if (heatmap.CurrentMonthIndex is int explicitIndex)
        {
            return explicitIndex >= 0 && explicitIndex < heatmap.Months.Count ? explicitIndex : -1;
        }

        var abbr = new DateTime(today.Year, today.Month, 1)
            .ToString("MMM", System.Globalization.CultureInfo.InvariantCulture);
        for (var i = 0; i < heatmap.Months.Count; i++)
        {
            if (string.Equals(heatmap.Months[i], abbr, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }
}