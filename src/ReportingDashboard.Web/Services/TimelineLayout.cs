namespace ReportingDashboard.Web.Services;

public static class TimelineLayout
{
    public static double ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)
    {
        if (rangeEnd <= rangeStart || pixelWidth <= 0)
        {
            return 0;
        }

        var total = (rangeEnd.ToDateTime(TimeOnly.MinValue) - rangeStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var offset = (date.ToDateTime(TimeOnly.MinValue) - rangeStart.ToDateTime(TimeOnly.MinValue)).TotalDays;
        if (total <= 0)
        {
            return 0;
        }
        return (offset / total) * pixelWidth;
    }

    public static int ComputeTrackY(int trackIndex) => 42 + (trackIndex * 56);

    public static int ComputeSvgHeight(int trackCount) => (trackCount * 56) + 40;
}