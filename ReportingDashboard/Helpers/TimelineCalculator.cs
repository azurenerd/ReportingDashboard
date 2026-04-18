using System.Globalization;

namespace ReportingDashboard.Helpers;

public static class TimelineCalculator
{
    public static double DateToX(DateOnly date, DateOnly start, DateOnly end, double svgWidth)
    {
        var totalDays = end.DayNumber - start.DayNumber;
        if (totalDays <= 0) return 0;
        var days = date.DayNumber - start.DayNumber;
        var x = (double)days / totalDays * svgWidth;
        return Math.Clamp(x, 0, svgWidth);
    }

    public static string DiamondPoints(double cx, double cy, double halfSize)
    {
        var c = CultureInfo.InvariantCulture;
        return string.Format(c, "{0},{1} {2},{3} {4},{5} {6},{7}",
            F(cx), F(cy - halfSize),
            F(cx + halfSize), F(cy),
            F(cx), F(cy + halfSize),
            F(cx - halfSize), F(cy));
    }

    public static double[] DistributeYPositions(int count, double svgHeight)
    {
        if (count <= 0) return Array.Empty<double>();
        var step = svgHeight / (count + 1);
        var positions = new double[count];
        for (int i = 0; i < count; i++)
            positions[i] = step * (i + 1);
        return positions;
    }

    // Format double for SVG attribute output (invariant culture)
    public static string F(double v) =>
        v.ToString("F2", CultureInfo.InvariantCulture);
}