using System.Globalization;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Pure, stateless coordinate math helpers used by the timeline and heatmap
/// layout engines. No UI, no DI, no state - deterministic from inputs.
/// </summary>
public static class TimelineMath
{
    private static readonly string[] MonthAbbrev =
    {
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    /// <summary>
    /// Maps a date into an x-coordinate on a timeline of the given width,
    /// proportional to the date's position within [start, end].
    /// </summary>
    public static double DateToX(DateOnly date, DateOnly start, DateOnly end, double width)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (end <= start) throw new ArgumentException("end must be after start", nameof(end));

        var total = (end.DayNumber - start.DayNumber);
        var offset = (date.DayNumber - start.DayNumber);
        return offset / (double)total * width;
    }

    /// <summary>
    /// Returns one gridline per month boundary in [start, end], first at x=0
    /// and subsequent months at their proportional x-coordinate.
    /// </summary>
    public static IReadOnlyList<MonthGridline> MonthGridlines(
        DateOnly start, DateOnly end, double width)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (end <= start) throw new ArgumentException("end must be after start", nameof(end));

        var list = new List<MonthGridline>();
        var cursor = new DateOnly(start.Year, start.Month, 1);
        while (cursor <= end)
        {
            var x = cursor < start ? 0.0 : DateToX(cursor, start, end, width);
            list.Add(new MonthGridline(x, MonthAbbrev[cursor.Month - 1]));
            cursor = cursor.AddMonths(1);
        }
        return list;
    }

    /// <summary>
    /// Computes the NOW marker position. If today falls outside [start, end]
    /// the marker is clamped to the nearest edge and InRange is false.
    /// </summary>
    public static NowMarker NowX(DateOnly today, DateOnly start, DateOnly end, double width)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (end <= start) throw new ArgumentException("end must be after start", nameof(end));

        if (today < start) return new NowMarker(0, false);
        if (today > end) return new NowMarker(width, false);
        return new NowMarker(DateToX(today, start, end, width), true);
    }

    /// <summary>
    /// Returns the index of the month (by 3-letter abbreviation, case-insensitive)
    /// in the supplied list that matches <paramref name="today"/>, or -1 if no match.
    /// </summary>
    public static int CurrentMonthIndex(DateOnly today, IReadOnlyList<string> months)
    {
        ArgumentNullException.ThrowIfNull(months);
        var target = MonthAbbrev[today.Month - 1];
        for (var i = 0; i < months.Count; i++)
        {
            var m = months[i];
            if (string.IsNullOrEmpty(m)) continue;
            var prefix = m.Length >= 3 ? m[..3] : m;
            if (string.Equals(prefix, target, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Truncates a list of items to at most <paramref name="max"/> rendered slots.
    /// If <c>items.Count &gt; max</c>, keeps the first <c>max - 1</c> items and
    /// returns an overflow count of <c>items.Count - (max - 1)</c> so callers can
    /// render a trailing "+K more" row. Otherwise returns the items unchanged with
    /// overflow 0.
    /// </summary>
    public static (IReadOnlyList<T> Kept, int OverflowCount) TruncateItems<T>(
        IReadOnlyList<T> items, int max)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (max < 1) throw new ArgumentOutOfRangeException(nameof(max), "max must be >= 1");

        if (items.Count <= max)
            return (items, 0);

        var keep = max - 1;
        var kept = new List<T>(keep);
        for (var i = 0; i < keep; i++) kept.Add(items[i]);
        var overflow = items.Count - keep;
        return (kept, overflow);
    }

    internal static string FormatOverflow(int count, IFormatProvider? culture = null) =>
        $"+{count.ToString(culture ?? CultureInfo.InvariantCulture)} more";
}