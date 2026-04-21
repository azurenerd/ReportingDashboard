using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed record HeatmapCellView(
    IReadOnlyList<string> Items,
    int OverflowCount,
    bool IsEmpty);

public sealed record HeatmapRowView(
    HeatmapCategory Category,
    string HeaderLabel,
    IReadOnlyList<HeatmapCellView> Cells);

public sealed record HeatmapViewModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyList<HeatmapRowView> Rows)
{
    public static HeatmapViewModel Empty { get; } = new(
        new[] { "Jan", "Feb", "Mar", "Apr" },
        -1,
        new[]
        {
            new HeatmapRowView(HeatmapCategory.Shipped, "SHIPPED", CreateEmptyCells(4)),
            new HeatmapRowView(HeatmapCategory.InProgress, "IN PROGRESS", CreateEmptyCells(4)),
            new HeatmapRowView(HeatmapCategory.Carryover, "CARRYOVER", CreateEmptyCells(4)),
            new HeatmapRowView(HeatmapCategory.Blockers, "BLOCKERS", CreateEmptyCells(4)),
        });

    private static IReadOnlyList<HeatmapCellView> CreateEmptyCells(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => new HeatmapCellView(Array.Empty<string>(), 0, true))
            .ToArray();
}

public static class HeatmapLayoutEngine
{
    private static readonly HeatmapCategory[] CategoryOrder =
    {
        HeatmapCategory.Shipped,
        HeatmapCategory.InProgress,
        HeatmapCategory.Carryover,
        HeatmapCategory.Blockers
    };

    public static HeatmapViewModel Build(
        Heatmap heatmap, DateOnly today, int defaultMaxItems = 4)
    {
        var maxItems = heatmap.MaxItemsPerCell >= 1 ? heatmap.MaxItemsPerCell : defaultMaxItems;

        int currentMonthIndex;
        if (heatmap.CurrentMonthIndex.HasValue)
        {
            currentMonthIndex = heatmap.CurrentMonthIndex.Value;
        }
        else
        {
            var monthAbbr = today.ToString("MMM");
            currentMonthIndex = -1;
            for (int i = 0; i < heatmap.Months.Count; i++)
            {
                if (string.Equals(heatmap.Months[i], monthAbbr, StringComparison.OrdinalIgnoreCase))
                {
                    currentMonthIndex = i;
                    break;
                }
            }
        }

        var rows = new List<HeatmapRowView>();
        foreach (var category in CategoryOrder)
        {
            var sourceRow = heatmap.Rows.FirstOrDefault(r => r.Category == category);
            var cells = new List<HeatmapCellView>();

            for (int i = 0; i < heatmap.Months.Count; i++)
            {
                IReadOnlyList<string> items = sourceRow?.Cells != null && i < sourceRow.Cells.Count
                    ? sourceRow.Cells[i]
                    : Array.Empty<string>();

                if (items.Count == 0)
                {
                    cells.Add(new HeatmapCellView(Array.Empty<string>(), 0, true));
                }
                else if (items.Count <= maxItems)
                {
                    cells.Add(new HeatmapCellView(items.ToList(), 0, false));
                }
                else
                {
                    var visible = items.Take(maxItems - 1).ToList();
                    var overflow = items.Count - (maxItems - 1);
                    cells.Add(new HeatmapCellView(visible, overflow, false));
                }
            }

            var headerLabel = category switch
            {
                HeatmapCategory.Shipped => "SHIPPED",
                HeatmapCategory.InProgress => "IN PROGRESS",
                HeatmapCategory.Carryover => "CARRYOVER",
                HeatmapCategory.Blockers => "BLOCKERS",
                _ => category.ToString().ToUpperInvariant()
            };

            rows.Add(new HeatmapRowView(category, headerLabel, cells));
        }

        return new HeatmapViewModel(heatmap.Months, currentMonthIndex, rows);
    }
}