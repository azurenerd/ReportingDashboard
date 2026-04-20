namespace ReportingDashboard.Web.Services;

public static class MilestoneLayout
{
    public const int SvgWidth = 1560;
    public const int SvgHeight = 185;
    public const int FirstLaneY = 42;
    public const int LastLaneY = 154;

    public static double DateToX(DateOnly date, DateOnly start, DateOnly end, int width = SvgWidth)
    {
        var total = (end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays;
        if (total <= 0) return 0;
        var offset = (date.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return (offset / total) * width;
    }

    public static (double X, bool Clamped) DateToClampedX(DateOnly date, DateOnly start, DateOnly end, int width = SvgWidth)
    {
        var x = DateToX(date, start, end, width);
        if (x < 0) return (0, true);
        if (x > width) return (width, true);
        return (x, false);
    }

    public static int SwimlaneY(int index, int count)
    {
        if (count <= 1) return 98;
        return FirstLaneY + index * ((LastLaneY - FirstLaneY) / Math.Max(1, count - 1));
    }

    public static IEnumerable<double> MonthBoundaries(DateOnly start, DateOnly end, int width = SvgWidth)
    {
        var cursor = new DateOnly(start.Year, start.Month, 1);
        if (cursor < start) cursor = cursor.AddMonths(1);
        while (cursor <= end)
        {
            yield return DateToX(cursor, start, end, width);
            cursor = cursor.AddMonths(1);
        }
    }

    public static IEnumerable<(string Label, double X)> MonthTicks(DateOnly start, DateOnly end, int width = SvgWidth)
    {
        var cursor = new DateOnly(start.Year, start.Month, 1);
        while (cursor <= end)
        {
            var x = DateToX(cursor, start, end, width);
            yield return (cursor.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture), x);
            cursor = cursor.AddMonths(1);
        }
    }
}