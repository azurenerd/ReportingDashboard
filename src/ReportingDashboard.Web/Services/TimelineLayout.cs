namespace ReportingDashboard.Web.Services;

public static class TimelineLayout
{
    public static double ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)
    {
        if (rangeEnd <= rangeStart || pixelWidth <= 0)
        {
            return 0d;
        }

        var total = (rangeEnd.DayNumber - rangeStart.DayNumber);
        var offset = (date.DayNumber - rangeStart.DayNumber);
        if (total <= 0)
        {
            return 0d;
        }
        return (double)offset / total * pixelWidth;
    }

    public static int ComputeTrackY(int trackIndex) => 42 + (trackIndex * 56);

    public static int ComputeSvgHeight(int trackCount) => (trackCount * 56) + 40;
}