namespace ReportingDashboard.Web.Models;

public sealed record HeatmapModel(
    IReadOnlyList<string> Months,
    int CurrentMonthIndex,
    IReadOnlyDictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>> Rows)
{
    public static HeatmapModel Empty { get; } = new(
        Months: new[] { "Jan", "Feb", "Mar", "Apr" },
        CurrentMonthIndex: 3,
        Rows: BuildEmptyRows(4));

    private static IReadOnlyDictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>> BuildEmptyRows(int monthCount)
    {
        var dict = new Dictionary<HeatmapCategory, IReadOnlyList<IReadOnlyList<string>>>();
        foreach (var cat in Enum.GetValues<HeatmapCategory>())
        {
            var cells = new List<IReadOnlyList<string>>(monthCount);
            for (var i = 0; i < monthCount; i++)
            {
                cells.Add(Array.Empty<string>());
            }
            dict[cat] = cells;
        }
        return dict;
    }
}