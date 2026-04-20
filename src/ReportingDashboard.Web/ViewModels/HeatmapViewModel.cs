using System.Collections.Generic;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.ViewModels;

public sealed record HeatmapViewModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyList<HeatmapRowView> Rows)
{
    public bool IsEmpty => Rows.Count == 0 || Months.Count == 0;

    public static HeatmapViewModel Empty { get; } = new(
        new[] { "", "", "", "" },
        -1,
        new[]
        {
            new HeatmapRowView(HeatmapCategory.Shipped,    "SHIPPED",     EmptyCells()),
            new HeatmapRowView(HeatmapCategory.InProgress, "IN PROGRESS", EmptyCells()),
            new HeatmapRowView(HeatmapCategory.Carryover,  "CARRYOVER",   EmptyCells()),
            new HeatmapRowView(HeatmapCategory.Blockers,   "BLOCKERS",    EmptyCells()),
        });

    private static IReadOnlyList<HeatmapCellView> EmptyCells() => new[]
    {
        HeatmapCellView.EmptyCell,
        HeatmapCellView.EmptyCell,
        HeatmapCellView.EmptyCell,
        HeatmapCellView.EmptyCell,
    };
}

public sealed record HeatmapRowView(
    HeatmapCategory Category,
    string HeaderLabel,
    IReadOnlyList<HeatmapCellView> Cells);

public sealed record HeatmapCellView(
    IReadOnlyList<string> Items,
    int OverflowCount,
    bool IsEmpty)
{
    public static HeatmapCellView EmptyCell { get; } =
        new(System.Array.Empty<string>(), 0, true);
}