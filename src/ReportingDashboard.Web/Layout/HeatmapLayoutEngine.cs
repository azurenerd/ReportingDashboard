using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Layout;

public static class HeatmapLayoutEngine
{
    private static readonly HeatmapCategory[] FixedOrder =
    {
        HeatmapCategory.Shipped,
        HeatmapCategory.InProgress,
        HeatmapCategory.Carryover,
        HeatmapCategory.Blockers,
    };

    public static HeatmapViewModel Build(Heatmap heatmap, DateOnly today, int defaultMaxItems = 4)
    {
        ArgumentNullException.ThrowIfNull(heatmap);

        var months = heatmap.Months ?? (IReadOnlyList<string>)Array.Empty<string>();
        int monthCount = months.Count;
        int maxItems = heatmap.MaxItemsPerCell >= 1 ? heatmap.MaxItemsPerCell : defaultMaxItems;

        var rowsByCategory = new Dictionary<HeatmapCategory, HeatmapRow>();
        if (heatmap.Rows is not null)
        {
            foreach (var row in heatmap.Rows)
            {
                rowsByCategory[row.Category] = row;
            }
        }

        var rowViews = new List<HeatmapRowView>(FixedOrder.Length);
        foreach (var cat in FixedOrder)
        {
            var header = HeaderLabel(cat);
            if (rowsByCategory.TryGetValue(cat, out var row) && row.Cells is not null)
            {
                var cells = new List<HeatmapCellView>(monthCount);
                for (int i = 0; i < monthCount; i++)
                {
                    var items = i < row.Cells.Count
                        ? (row.Cells[i] ?? (IReadOnlyList<string>)Array.Empty<string>())
                        : Array.Empty<string>();
                    cells.Add(BuildCell(items, maxItems));
                }
                rowViews.Add(new HeatmapRowView(cat, header, cells));
            }
            else
            {
                rowViews.Add(new HeatmapRowView(cat, header, EmptyCells(monthCount)));
            }
        }

        int currentIndex = ResolveCurrentMonthIndex(heatmap.CurrentMonthIndex, months, today);

        return new HeatmapViewModel(months, currentIndex, rowViews);
    }

    private static HeatmapCellView BuildCell(IReadOnlyList<string> items, int maxItems)
    {
        if (items.Count == 0)
        {
            return new HeatmapCellView(Array.Empty<string>(), 0, true);
        }

        if (items.Count > maxItems)
        {
            int keep = Math.Max(0, maxItems - 1);
            int k = items.Count - keep;
            var truncated = new List<string>(keep + 1);
            for (int i = 0; i < keep; i++) truncated.Add(items[i]);
            truncated.Add($"+{k} more");
            return new HeatmapCellView(truncated, k, false);
        }

        return new HeatmapCellView(items.ToArray(), 0, false);
    }

    private static IReadOnlyList<HeatmapCellView> EmptyCells(int count)
    {
        var list = new List<HeatmapCellView>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new HeatmapCellView(Array.Empty<string>(), 0, true));
        }
        return list;
    }

    private static int ResolveCurrentMonthIndex(int? explicitIndex, IReadOnlyList<string> months, DateOnly today)
    {
        if (explicitIndex is int idx && idx >= 0 && idx < months.Count)
        {
            return idx;
        }

        if (months.Count == 0) return -1;

        var abbr = CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(today.Month);
        for (int i = 0; i < months.Count; i++)
        {
            if (string.Equals(months[i], abbr, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    private static string HeaderLabel(HeatmapCategory c) => c switch
    {
        HeatmapCategory.Shipped => "SHIPPED",
        HeatmapCategory.InProgress => "IN PROGRESS",
        HeatmapCategory.Carryover => "CARRYOVER",
        HeatmapCategory.Blockers => "BLOCKERS",
        _ => c.ToString().ToUpperInvariant(),
    };
}