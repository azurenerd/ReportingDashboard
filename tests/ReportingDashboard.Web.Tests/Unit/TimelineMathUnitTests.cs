using System;
using System.Collections.Generic;
using FluentAssertions;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class TimelineMathUnitTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 6, 30);
    private const double Width = 1560.0;

    [Fact]
    public void DateToX_Boundaries_And_CanonicalSpecValues()
    {
        TimelineMath.DateToX(Start, Start, End, Width).Should().Be(0.0);
        TimelineMath.DateToX(End, Start, End, Width).Should().Be(Width);

        // Implementation uses DayNumber diff: total = 180 days (Jan 1 -> Jun 30).
        // Apr 19 offset = 108 days -> 108/180 * 1560 = 936.0
        TimelineMath.DateToX(new DateOnly(2026, 4, 19), Start, End, Width)
            .Should().BeApproximately(936.0, 2.0);

        // May 19 offset = 138 days -> 138/180 * 1560 = 1196.0
        TimelineMath.DateToX(new DateOnly(2026, 5, 19), Start, End, Width)
            .Should().BeApproximately(1196.0, 2.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void DateToX_InvalidInputs_Throw(double badWidth)
    {
        Action badW = () => TimelineMath.DateToX(Start, Start, End, badWidth);
        badW.Should().Throw<ArgumentOutOfRangeException>();

        Action startEqEnd = () => TimelineMath.DateToX(Start, Start, Start, Width);
        startEqEnd.Should().Throw<ArgumentException>();

        Action startAfterEnd = () => TimelineMath.DateToX(Start, End, Start, Width);
        startAfterEnd.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MonthGridlines_JanToJun_ReturnsSixEntries_FirstAtZero()
    {
        var gridlines = TimelineMath.MonthGridlines(Start, End, Width);

        gridlines.Should().HaveCount(6);
        gridlines[0].X.Should().Be(0.0);

        var labels = new List<string>();
        foreach (var g in gridlines) labels.Add(g.Label);
        labels.Should().Equal("Jan", "Feb", "Mar", "Apr", "May", "Jun");
    }

    [Fact]
    public void NowX_BehavesInsideAtEdgesAndOutside()
    {
        var today = new DateOnly(2026, 4, 19);
        var inside = TimelineMath.NowX(today, Start, End, Width);
        inside.InRange.Should().BeTrue();
        inside.X.Should().Be(TimelineMath.DateToX(today, Start, End, Width));

        var before = TimelineMath.NowX(new DateOnly(2025, 12, 1), Start, End, Width);
        before.InRange.Should().BeFalse();
        before.X.Should().Be(0.0);

        var after = TimelineMath.NowX(new DateOnly(2026, 9, 1), Start, End, Width);
        after.InRange.Should().BeFalse();
        after.X.Should().Be(Width);

        TimelineMath.NowX(Start, Start, End, Width).InRange.Should().BeTrue();
        TimelineMath.NowX(End, Start, End, Width).InRange.Should().BeTrue();
    }

    [Fact]
    public void CurrentMonthIndex_And_TruncateItems_BehaveCorrectly()
    {
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 4, 19),
            new[] { "Jan", "Feb", "Mar", "Apr" }).Should().Be(3);
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 4, 19),
            new[] { "JAN", "feb", "Mar", "APR" }).Should().Be(3);
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 7, 1),
            new[] { "Jan", "Feb", "Mar", "Apr" }).Should().Be(-1);
        TimelineMath.CurrentMonthIndex(new DateOnly(2026, 4, 1),
            Array.Empty<string>()).Should().Be(-1);

        var empty = TimelineMath.TruncateItems(Array.Empty<int>(), 4);
        empty.Kept.Should().BeEmpty();
        empty.OverflowCount.Should().Be(0);

        var four = new[] { 1, 2, 3, 4 };
        var r4 = TimelineMath.TruncateItems(four, 4);
        r4.Kept.Should().Equal(four);
        r4.OverflowCount.Should().Be(0);

        var seven = new[] { 1, 2, 3, 4, 5, 6, 7 };
        var snapshot = (int[])seven.Clone();
        var r7 = TimelineMath.TruncateItems(seven, 4);
        r7.Kept.Should().Equal(1, 2, 3);
        r7.OverflowCount.Should().Be(4);
        seven.Should().Equal(snapshot);

        Action nullItems = () => TimelineMath.TruncateItems<int>(null!, 4);
        nullItems.Should().Throw<ArgumentNullException>();

        Action zeroMax = () => TimelineMath.TruncateItems(four, 0);
        zeroMax.Should().Throw<ArgumentOutOfRangeException>();

        Action negMax = () => TimelineMath.TruncateItems(four, -1);
        negMax.Should().Throw<ArgumentOutOfRangeException>();
    }
}