using System;
using System.Collections.Generic;
using System.Globalization;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.ViewModels;

namespace ReportingDashboard.Web.Layout;

public static class HeatmapLayoutEngine
{
    private static readonly HeatmapCategory[] RowOrder =
    {
        HeatmapCategory.Shipped,
        HeatmapCategory.InProgress,
        HeatmapCategory.Carryover,
        HeatmapCategory.Blockers,
    };

    public static HeatmapViewModel Build(Heatmap heatmap, DateOnly today, int defaultMaxItems = 4)
    {
        ArgumentNullException.ThrowIfNull(heatmap);

        var months = heatmap.Months ?? Array.Empty<string>();
        var maxItems = heatmap.MaxItemsPerCell > 0 ? heatmap.MaxItemsPerCell : defaultMaxItems;
        if (maxItems < 1) maxItems = 1;

        int currentIndex = ResolveCurrentMonthIndex(heatmap.CurrentMonthIndex, months, today);

        var rowsByCategory = new Dictionary<HeatmapCategory, HeatmapRow>();
        if (heatmap.Rows is not null)
        {
            foreach (var row in heatmap.Rows)
            {
                rowsByCategory[row.Category] = row;
            }
        }

        var resultRows = new List<HeatmapRowView>(RowOrder.Length);
        foreach (var category in RowOrder)
        {
            rowsByCategory.TryGetValue(category, out var row);
            var cells = BuildCells(row, months.Count, maxItems);
            resultRows.Add(new HeatmapRowView(category, HeaderLabel(category), cells));
        }

        return new HeatmapViewModel(months, currentIndex, resultRows);
    }

    private static int ResolveCurrentMonthIndex(int? explicitIndex, IReadOnlyList<string> months, DateOnly today)
    {
        if (explicitIndex is int idx)
        {
            return (idx >= 0 && idx < months.Count) ? idx : -1;
        }

        var abbrev = today.ToString("MMM", CultureInfo.InvariantCulture);
        for (int i = 0; i < months.Count; i++)
        {
            if (string.Equals(months[i], abbrev, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    private static IReadOnlyList<HeatmapCellView> BuildCells(HeatmapRow? row, int monthCount, int maxItems)
    {
        var cells = new List<HeatmapCellView>(monthCount);
        for (int i = 0; i < monthCount; i++)
        {
            IReadOnlyList<string>? raw = null;
            if (row?.Cells is not null && i < row.Cells.Count)
            {
                raw = row.Cells[i];
            }

            cells.Add(BuildCell(raw, maxItems));
        }
        return cells;
    }

    private static HeatmapCellView BuildCell(IReadOnlyList<string>? items, int maxItems)
    {
        if (items is null || items.Count == 0)
        {
            return HeatmapCellView.EmptyCell;
        }

        if (items.Count <= maxItems)
        {
            var copy = new List<string>(items.Count);
            for (int i = 0; i < items.Count; i++) copy.Add(items[i] ?? string.Empty);
            return new HeatmapCellView(copy, 0, false);
        }

        int keep = maxItems - 1;
        if (keep < 0) keep = 0;
        int overflow = items.Count - keep;

        var kept = new List<string>(keep);
        for (int i = 0; i < keep; i++) kept.Add(items[i] ?? string.Empty);
        return new HeatmapCellView(kept, overflow, false);
    }

    private static string HeaderLabel(HeatmapCategory category) => category switch
    {
        HeatmapCategory.Shipped => "SHIPPED",
        HeatmapCategory.InProgress => "IN PROGRESS",
        HeatmapCategory.Carryover => "CARRYOVER",
        HeatmapCategory.Blockers => "BLOCKERS",
        _ => category.ToString().ToUpperInvariant(),
    };
}