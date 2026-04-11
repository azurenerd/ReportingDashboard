using FluentAssertions;
using ReportingDashboard.Models;
using System.Reflection;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the private helper methods in Timeline.razor using reflection.
/// These validate the core calculation logic (DateToX, DiamondPoints, ParseDate, GetMonthPositions).
/// </summary>
[Trait("Category", "Unit")]
public class TimelineHelperTests
{
    private static readonly Type TimelineType = typeof(ReportingDashboard.Components.Timeline);

    private static MethodInfo GetStaticMethod(string name)
    {
        var method = TimelineType.GetMethod(name,
            BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull($"Method '{name}' should exist on Timeline component");
        return method!;
    }

    #region ParseDate Tests

    [Fact]
    public void ParseDate_ValidIsoDate_ReturnsCorrectDateTime()
    {
        var method = GetStaticMethod("ParseDate");
        var result = (DateTime)method.Invoke(null, new object[] { "2026-04-10" })!;

        result.Year.Should().Be(2026);
        result.Month.Should().Be(4);
        result.Day.Should().Be(10);
    }

    [Fact]
    public void ParseDate_InvalidDate_ReturnsFallback()
    {
        var method = GetStaticMethod("ParseDate");
        var result = (DateTime)method.Invoke(null, new object[] { "not-a-date" })!;

        // Falls back to DateTime.Now
        result.Date.Should().Be(DateTime.Now.Date);
    }

    [Fact]
    public void ParseDate_EmptyString_ReturnsFallback()
    {
        var method = GetStaticMethod("ParseDate");
        var result = (DateTime)method.Invoke(null, new object[] { "" })!;

        result.Date.Should().Be(DateTime.Now.Date);
    }

    [Fact]
    public void ParseDate_StartOfYear_ParsesCorrectly()
    {
        var method = GetStaticMethod("ParseDate");
        var result = (DateTime)method.Invoke(null, new object[] { "2026-01-01" })!;

        result.Should().Be(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void ParseDate_EndOfYear_ParsesCorrectly()
    {
        var method = GetStaticMethod("ParseDate");
        var result = (DateTime)method.Invoke(null, new object[] { "2026-12-31" })!;

        result.Should().Be(new DateTime(2026, 12, 31));
    }

    [Fact]
    public void ParseDate_LeapYearDate_ParsesCorrectly()
    {
        var method = GetStaticMethod("ParseDate");
        var result = (DateTime)method.Invoke(null, new object[] { "2028-02-29" })!;

        result.Should().Be(new DateTime(2028, 2, 29));
    }

    #endregion

    #region DateToX Tests

    [Fact]
    public void DateToX_AtStartDate_ReturnsZero()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var totalDays = (new DateTime(2026, 6, 30) - start).TotalDays;

        var result = (double)method.Invoke(null, new object[] { start, start, totalDays, 1560 })!;

        result.Should().Be(0.0);
    }

    [Fact]
    public void DateToX_AtEndDate_ReturnsSvgWidth()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var totalDays = (end - start).TotalDays;

        var result = (double)method.Invoke(null, new object[] { end, start, totalDays, 1560 })!;

        result.Should().Be(1560.0);
    }

    [Fact]
    public void DateToX_AtMidpoint_ReturnsApproximatelyHalfWidth()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var totalDays = (end - start).TotalDays;
        var mid = start.AddDays(totalDays / 2);

        var result = (double)method.Invoke(null, new object[] { mid, start, totalDays, 1560 })!;

        result.Should().BeApproximately(780.0, 1.0);
    }

    [Fact]
    public void DateToX_BeforeStartDate_ReturnsNegative()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var totalDays = (end - start).TotalDays;
        var before = new DateTime(2025, 12, 1);

        var result = (double)method.Invoke(null, new object[] { before, start, totalDays, 1560 })!;

        result.Should().BeNegative();
    }

    [Fact]
    public void DateToX_AfterEndDate_ReturnsGreaterThanWidth()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var totalDays = (end - start).TotalDays;
        var after = new DateTime(2026, 8, 1);

        var result = (double)method.Invoke(null, new object[] { after, start, totalDays, 1560 })!;

        result.Should().BeGreaterThan(1560.0);
    }

    [Fact]
    public void DateToX_ReturnsRoundedValue()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var totalDays = (end - start).TotalDays;
        var date = new DateTime(2026, 4, 10);

        var result = (double)method.Invoke(null, new object[] { date, start, totalDays, 1560 })!;

        // Should be rounded to 1 decimal
        var rounded = Math.Round(result, 1);
        result.Should().Be(rounded);
    }

    [Fact]
    public void DateToX_NowDateApril10_IsBetweenAprAndMayGridLines()
    {
        var method = GetStaticMethod("DateToX");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);
        var totalDays = (end - start).TotalDays;
        var nowDate = new DateTime(2026, 4, 10);
        var aprStart = new DateTime(2026, 4, 1);
        var mayStart = new DateTime(2026, 5, 1);

        var nowX = (double)method.Invoke(null, new object[] { nowDate, start, totalDays, 1560 })!;
        var aprX = (double)method.Invoke(null, new object[] { aprStart, start, totalDays, 1560 })!;
        var mayX = (double)method.Invoke(null, new object[] { mayStart, start, totalDays, 1560 })!;

        nowX.Should().BeGreaterThan(aprX);
        nowX.Should().BeLessThan(mayX);
    }

    #endregion

    #region DiamondPoints Tests

    [Fact]
    public void DiamondPoints_ReturnsCorrectFormat()
    {
        var method = GetStaticMethod("DiamondPoints");
        var result = (string)method.Invoke(null, new object[] { 100.0, 50.0, 9.0 })!;

        // Format: "cx,cy-r cx+r,cy cx,cy+r cx-r,cy"
        result.Should().Be("100,41 109,50 100,59 91,50");
    }

    [Fact]
    public void DiamondPoints_AtOrigin_ReturnsSymmetricPoints()
    {
        var method = GetStaticMethod("DiamondPoints");
        var result = (string)method.Invoke(null, new object[] { 0.0, 0.0, 10.0 })!;

        result.Should().Be("0,-10 10,0 0,10 -10,0");
    }

    [Fact]
    public void DiamondPoints_SmallRadius_ReturnsClosePoints()
    {
        var method = GetStaticMethod("DiamondPoints");
        var result = (string)method.Invoke(null, new object[] { 500.0, 100.0, 1.0 })!;

        result.Should().Be("500,99 501,100 500,101 499,100");
    }

    [Fact]
    public void DiamondPoints_HasFourVertices()
    {
        var method = GetStaticMethod("DiamondPoints");
        var result = (string)method.Invoke(null, new object[] { 200.0, 80.0, 11.0 })!;

        var pairs = result.Split(' ');
        pairs.Should().HaveCount(4);
    }

    [Fact]
    public void DiamondPoints_VerticesFormRhombus()
    {
        var method = GetStaticMethod("DiamondPoints");
        double cx = 300, cy = 150, r = 9;
        var result = (string)method.Invoke(null, new object[] { cx, cy, r })!;

        var pairs = result.Split(' ');
        // Top: cx, cy-r
        pairs[0].Should().Be($"{cx},{cy - r}");
        // Right: cx+r, cy
        pairs[1].Should().Be($"{cx + r},{cy}");
        // Bottom: cx, cy+r
        pairs[2].Should().Be($"{cx},{cy + r}");
        // Left: cx-r, cy
        pairs[3].Should().Be($"{cx - r},{cy}");
    }

    #endregion

    #region GetMonthPositions Tests

    [Fact]
    public void GetMonthPositions_JanToJun_ReturnsSixOrSevenMonths()
    {
        var method = GetStaticMethod("GetMonthPositions");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        var result = method.Invoke(null, new object[] { start, end, 1560 });
        var positions = (System.Collections.IList)result!;

        // Jan through Jun = 6 months (Jul 1 > end, so stops at Jun)
        positions.Count.Should().BeGreaterOrEqualTo(6);
    }

    [Fact]
    public void GetMonthPositions_FirstMonth_StartsAtZeroX()
    {
        var method = GetStaticMethod("GetMonthPositions");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        var result = method.Invoke(null, new object[] { start, end, 1560 });
        var positions = (System.Collections.IList)result!;

        // First position should be at x=0 since start is Jan 1
        var first = positions[0]!;
        var xProp = first.GetType().GetProperty("X");
        var x = (double)xProp!.GetValue(first)!;
        x.Should().Be(0.0);
    }

    [Fact]
    public void GetMonthPositions_MonthLabels_AreThreeLetterAbbreviations()
    {
        var method = GetStaticMethod("GetMonthPositions");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 3, 31);

        var result = method.Invoke(null, new object[] { start, end, 1560 });
        var positions = (System.Collections.IList)result!;

        var labels = new List<string>();
        foreach (var pos in positions)
        {
            var labelProp = pos.GetType().GetProperty("Label");
            labels.Add((string)labelProp!.GetValue(pos)!);
        }

        labels.Should().Contain("Jan");
        labels.Should().Contain("Feb");
        labels.Should().Contain("Mar");
    }

    [Fact]
    public void GetMonthPositions_SingleMonth_ReturnsOnePosition()
    {
        var method = GetStaticMethod("GetMonthPositions");
        var start = new DateTime(2026, 3, 1);
        var end = new DateTime(2026, 3, 31);

        var result = method.Invoke(null, new object[] { start, end, 1560 });
        var positions = (System.Collections.IList)result!;

        positions.Count.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public void GetMonthPositions_XPositionsAreIncreasing()
    {
        var method = GetStaticMethod("GetMonthPositions");
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        var result = method.Invoke(null, new object[] { start, end, 1560 });
        var positions = (System.Collections.IList)result!;

        var xValues = new List<double>();
        foreach (var pos in positions)
        {
            var xProp = pos.GetType().GetProperty("X");
            xValues.Add((double)xProp!.GetValue(pos)!);
        }

        for (int i = 1; i < xValues.Count; i++)
        {
            xValues[i].Should().BeGreaterThan(xValues[i - 1]);
        }
    }

    #endregion
}