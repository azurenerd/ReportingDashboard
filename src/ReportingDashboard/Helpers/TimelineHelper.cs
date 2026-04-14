using System.Globalization;
using System.Net;

namespace ReportingDashboard.Helpers;

public static class TimelineHelper
{
    public const double SvgWidth = 1560.0;

    public static double CalculateX(string dateStr, DateOnly start, DateOnly end)
    {
        var date = DateOnly.ParseExact(dateStr, "yyyy-MM-dd");
        return CalculateX(date, start, end);
    }

    public static double CalculateX(DateOnly date, DateOnly start, DateOnly end)
    {
        var totalDays = end.DayNumber - start.DayNumber;
        if (totalDays == 0) return 0;
        var elapsed = date.DayNumber - start.DayNumber;
        return (double)elapsed / totalDays * SvgWidth;
    }

    public static double CalculateXClamped(DateOnly date, DateOnly start, DateOnly end)
    {
        var x = CalculateX(date, start, end);
        return Math.Clamp(x, 0, SvgWidth);
    }

    public static List<(string Label, double X)> GetMonthGridlines(DateOnly start, DateOnly end)
    {
        var results = new List<(string, double)>();
        var current = new DateOnly(start.Year, start.Month, 1);
        if (current <= start) current = current.AddMonths(1);

        while (current <= end)
        {
            var x = CalculateX(current, start, end);
            results.Add((current.ToString("MMM"), x));
            current = current.AddMonths(1);
        }
        return results;
    }

    public static double GetYLane(int index, int total)
    {
        if (total <= 0) return 98;
        if (total == 1) return 98;
        if (total == 2) return index == 0 ? 70 : 126;
        if (total == 3) return index switch { 0 => 42, 1 => 98, _ => 154 };
        double minY = 30, maxY = 170;
        return minY + (maxY - minY) * index / (total - 1);
    }

    public static bool GetLabelAbove(string? labelPosition, int index)
    {
        if (labelPosition == "above") return true;
        if (labelPosition == "below") return false;
        return index % 2 == 0;
    }

    public static string DiamondPoints(double cx, double cy, double half)
    {
        var ic = CultureInfo.InvariantCulture;
        return string.Format(ic,
            "{0:F1},{1:F1} {2:F1},{3:F1} {4:F1},{5:F1} {6:F1},{7:F1}",
            cx, cy - half, cx + half, cy, cx, cy + half, cx - half, cy);
    }

    public static string F(double val)
    {
        return val.ToString("F1", CultureInfo.InvariantCulture);
    }

    public static string RenderSvgText(double x, double y, string fill, string fontSize,
        string fontWeight, string content, string? textAnchor = null)
    {
        var ic = CultureInfo.InvariantCulture;
        var anchor = textAnchor != null ? $" text-anchor=\"{textAnchor}\"" : "";
        var encoded = WebUtility.HtmlEncode(content);
        return $"<text x=\"{x.ToString("F1", ic)}\" y=\"{y.ToString("F1", ic)}\" fill=\"{fill}\" font-size=\"{fontSize}\" font-weight=\"{fontWeight}\" font-family=\"Segoe UI,Arial\"{anchor}>{encoded}</text>";
    }
}