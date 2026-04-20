namespace ReportingDashboard.Web.Services;

// Stub — downstream task T3 will implement the full date→pixel math.
public static class TimelineLayout
{
    public static double ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)
    {
        if (rangeEnd <= rangeStart || pixelWidth <= 0)
        {
            return 0;
        }

        var total = (rangeEnd.DayNumber - rangeStart.DayNumber);
        var offset = (date.DayNumber - rangeStart.DayNumber);
        var ratio = (double)offset / total;
        return ratio * pixelWidth;
    }

    public static int ComputeTrackY(int trackIndex) => 42 + (trackIndex * 56);

    public static int ComputeSvgHeight(int trackCount) => Math.Max(1, trackCount) * 56 + 40;
}