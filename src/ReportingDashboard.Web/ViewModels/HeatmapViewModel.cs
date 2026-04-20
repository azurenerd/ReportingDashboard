using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.ViewModels;

public sealed record HeatmapViewModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyList<HeatmapRowView> Rows)
{
    public static HeatmapViewModel Empty { get; } = new(
        Array.Empty<string>(),
        -1,
        Array.Empty<HeatmapRowView>());
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
    public static HeatmapCellView EmptyCell { get; } = new(Array.Empty<string>(), 0, true);
}