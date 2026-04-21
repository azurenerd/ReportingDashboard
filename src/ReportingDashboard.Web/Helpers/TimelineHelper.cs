using System.Globalization;

namespace ReportingDashboard.Web.Helpers;

public static class TimelineHelper
{
    public static double DateToX(string dateStr, string startDateStr, string endDateStr, double totalWidth)
    {
        if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return 0;
        if (!DateTime.TryParse(startDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
            return 0;
        if (!DateTime.TryParse(endDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            return 0;

        if (end <= start)
            return 0;

        var totalDays = (end - start).TotalDays;
        var elapsed = (date - start).TotalDays;
        var ratio = elapsed / totalDays;

        return Math.Clamp(ratio * totalWidth, 0, totalWidth);
    }
}