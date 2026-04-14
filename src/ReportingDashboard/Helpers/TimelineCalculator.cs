using System.Globalization;

namespace ReportingDashboard.Helpers;

/// <summary>
/// Pure calculation logic for SVG timeline coordinate mapping.
/// Extracted for testability. The Timeline.razor component duplicates this
/// logic internally to avoid cross-namespace resolution issues in Razor compilation.
/// </summary>
public static class TimelineCalculator
{
    public const double SvgWidth = 1560;
    public const double SvgHeight = 185;
    public const double TopMargin = 28;

    private const double TrackStartY = TopMargin + 14.0;   // 42
    private const double TrackEndY = SvgHeight - 31.0;     // 154

    /// <summary>
    /// Maps a date to an X pixel coordinate within the SVG.
    /// Linear interpolation: x = (elapsed / total) * SvgWidth, clamped to [0, SvgWidth].
    /// </summary>
    public static double GetXPosition(DateTime date, DateTime timelineStart, DateTime timelineEnd)
    {
        var totalDays = (timelineEnd - timelineStart).TotalDays;
        if (totalDays <= 0) return 0;
        var elapsed = (date - timelineStart).TotalDays;
        return Math.Clamp((elapsed / totalDays) * SvgWidth, 0, SvgWidth);
    }

    /// <summary>
    /// Returns the Y coordinate for a milestone track, evenly distributed within the SVG.
    /// For 3 tracks: y ≈ 42, 98, 154. Single track centers at y = 98.
    /// </summary>
    public static double GetTrackY(int trackIndex, int trackCount)
    {
        if (trackCount <= 1) return (TrackStartY + TrackEndY) / 2.0;
        return TrackStartY + (trackIndex * (TrackEndY - TrackStartY) / (trackCount - 1));
    }

    /// <summary>
    /// Generates SVG polygon points string for a diamond marker centered at (cx, cy).
    /// Points: top, right, bottom, left.
    /// </summary>
    public static string DiamondPoints(double cx, double cy, double r = 11)
    {
        return $"{F(cx)},{F(cy - r)} {F(cx + r)},{F(cy)} {F(cx)},{F(cy + r)} {F(cx - r)},{F(cy)}";
    }

    /// <summary>
    /// Formats a double for SVG attribute output using InvariantCulture.
    /// Ensures period decimal separator regardless of system locale.
    /// </summary>
    public static string F(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Generates month gridline positions for the 1st of each month within the timeline range.
    /// Skips months where the 1st falls before timelineStart.
    /// </summary>
    public static List<MonthGridline> GetMonthGridlines(DateTime timelineStart, DateTime timelineEnd)
    {
        var gridlines = new List<MonthGridline>();
        var current = new DateTime(timelineStart.Year, timelineStart.Month, 1);
        if (current < timelineStart)
            current = current.AddMonths(1);

        while (current <= timelineEnd)
        {
            gridlines.Add(new MonthGridline
            {
                Date = current,
                Label = current.ToString("MMM", CultureInfo.InvariantCulture)
            });
            current = current.AddMonths(1);
        }

        return gridlines;
    }

    public class MonthGridline
    {
        public DateTime Date { get; init; }
        public string Label { get; init; } = string.Empty;
    }
}