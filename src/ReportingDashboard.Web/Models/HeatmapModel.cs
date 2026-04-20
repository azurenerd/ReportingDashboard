namespace ReportingDashboard.Web.Models;

public sealed record HeatmapModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyDictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>> Rows)
{
    public static HeatmapModel Empty { get; } = new(
        Months: new[] { "Jan", "Feb", "Mar", "Apr" },
        CurrentMonthIndex: 0,
        Rows: new Dictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>>
        {
            [HeatmapCategory.Shipped] = EmptyRow(),
            [HeatmapCategory.InProgress] = EmptyRow(),
            [HeatmapCategory.Carryover] = EmptyRow(),
            [HeatmapCategory.Blockers] = EmptyRow(),
        });

    private static IReadOnlyList<IReadOnlyList<string>> EmptyRow() =>
        new IReadOnlyList<string>[]
        {
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
        };
}