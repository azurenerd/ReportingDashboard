namespace ReportingDashboard.Web.Models;

public sealed record HeatmapModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyDictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>> Rows)
{
    public static HeatmapModel Empty()
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr" };
        var emptyRow = (IReadOnlyList<IReadOnlyList<string>>)new IReadOnlyList<string>[]
        {
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>()
        };
        var rows = new Dictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>>
        {
            [HeatmapCategory.Shipped] = emptyRow,
            [HeatmapCategory.InProgress] = emptyRow,
            [HeatmapCategory.Carryover] = emptyRow,
            [HeatmapCategory.Blockers] = emptyRow
        };
        return new HeatmapModel(months, 0, rows);
    }
}