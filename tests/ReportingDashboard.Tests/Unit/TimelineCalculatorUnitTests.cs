using ReportingDashboard.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class TimelineCalculatorUnitTests
{
    private static readonly DateTime Start = new(2026, 1, 1);
    private static readonly DateTime End = new(2026, 7, 1);

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData(0, 0.0)]       // at start → 0
    [InlineData(181, 1560.0)]  // at end → SvgWidth (Jan 1 to Jul 1 = 181 days)
    public void GetXPosition_AtBoundaries_ReturnsExpectedEdge(int daysFromStart, double expected)
    {
        var date = Start.AddDays(daysFromStart);
        var result = TimelineCalculator.GetXPosition(date, Start, End);
        Assert.Equal(expected, result, precision: 2);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void GetXPosition_AtMidpoint_ReturnsHalfWidth()
    {
        var totalDays = (End - Start).TotalDays;
        var midpoint = Start.AddDays(totalDays / 2.0);

        var result = TimelineCalculator.GetXPosition(midpoint, Start, End);

        Assert.Equal(780.0, result, precision: 2);
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void GetXPosition_OutOfRange_ClampsToSvgBounds()
    {
        var beforeStart = Start.AddDays(-30);
        var afterEnd = End.AddDays(30);

        Assert.Equal(0.0, TimelineCalculator.GetXPosition(beforeStart, Start, End));
        Assert.Equal(1560.0, TimelineCalculator.GetXPosition(afterEnd, Start, End));
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void GetTrackY_VariousTrackCounts_ReturnsEvenlySpaced()
    {
        // Single track centers at (42 + 154) / 2 = 98
        Assert.Equal(98.0, TimelineCalculator.GetTrackY(0, 1), precision: 2);

        // Three tracks: 42, 98, 154
        Assert.Equal(42.0, TimelineCalculator.GetTrackY(0, 3), precision: 2);
        Assert.Equal(98.0, TimelineCalculator.GetTrackY(1, 3), precision: 2);
        Assert.Equal(154.0, TimelineCalculator.GetTrackY(2, 3), precision: 2);

        // Five tracks: all within [42, 154]
        for (int i = 0; i < 5; i++)
        {
            var y = TimelineCalculator.GetTrackY(i, 5);
            Assert.InRange(y, 42.0, 154.0);
        }
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DiamondPoints_GeneratesCorrectInvariantCulturePolygon()
    {
        var result = TimelineCalculator.DiamondPoints(100.5, 50.5, 11);

        // Expected: "100.5,39.5 111.5,50.5 100.5,61.5 89.5,50.5"
        Assert.Equal("100.5,39.5 111.5,50.5 100.5,61.5 89.5,50.5", result);

        // No comma-as-decimal (InvariantCulture check)
        var pairs = result.Split(' ');
        Assert.Equal(4, pairs.Length);
        foreach (var pair in pairs)
        {
            var coords = pair.Split(',');
            Assert.Equal(2, coords.Length);
            Assert.True(double.TryParse(coords[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _));
            Assert.True(double.TryParse(coords[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _));
        }
    }
}