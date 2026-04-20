namespace ReportingDashboard.Web.Services;

public static class TimelineLayout
{
    public const int SvgWidth = 1560;
    public const int TrackRowHeight = 56;
    public const int FirstTrackY = 42;

    public static double ComputeX(DateOnly date, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)
    {
        // Stub: real implementation owned by T3.
        _ = date;
        _ = rangeStart;
        _ = rangeEnd;
        _ = pixelWidth;
        return 0.0;
    }

    public static double ComputeNowX(DateOnly today, DateOnly rangeStart, DateOnly rangeEnd, int pixelWidth)
        => ComputeX(today, rangeStart, rangeEnd, pixelWidth);

    public static IReadOnlyList<(string Label, int X)> ComputeMonthGridlines(DateOnly rangeStart, DateOnly rangeEnd)
    {
        // Stub: real implementation owned by T3.
        _ = rangeStart;
        _ = rangeEnd;
        return Array.Empty<(string, int)>();
    }

    public static int ComputeTrackY(int trackIndex) => FirstTrackY + (trackIndex * TrackRowHeight);

    public static int ComputeSvgHeight(int trackCount) => (trackCount * TrackRowHeight) + 40;
}