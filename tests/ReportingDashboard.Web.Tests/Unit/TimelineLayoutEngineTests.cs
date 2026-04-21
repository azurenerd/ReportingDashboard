using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using ReportingDashboard.Web.Layout;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class TimelineLayoutEngineTests
{
    private static Timeline MakeTimeline(DateOnly start, DateOnly end, params TimelineLane[] lanes)
    {
        return new Timeline
        {
            Start = start,
            End = end,
            Lanes = lanes.ToList()
        };
    }

    private static TimelineLane Lane(string id, string label, string color, params Milestone[] ms)
    {
        return new TimelineLane
        {
            Id = id,
            Label = label,
            Color = color,
            Milestones = ms.ToList()
        };
    }

    [Theory]
    [InlineData("2026-04-19", 936.0)] // 108/180 * 1560
    [InlineData("2026-05-19", 1196.0)] // 138/180 * 1560
    [InlineData("2026-01-01", 0.0)]
    [InlineData("2026-06-30", 1560.0)]
    [InlineData("2026-04-01", 780.0)] // 90/180 * 1560
    public void Build_NowX_ForCanonicalFixture_MatchesFormula(string todayIso, double expectedX)
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 6, 30);
        var today = DateOnly.Parse(todayIso);
        var tl = MakeTimeline(start, end, Lane("M1", "L", "#0078D4"));

        var vm = TimelineLayoutEngine.Build(tl, today);

        vm.Now.InRange.Should().BeTrue();
        vm.Now.X.Should().BeApproximately(expectedX, 0.5);
    }

    [Theory]
    [InlineData("2025-12-31")]
    [InlineData("2026-07-01")]
    public void Build_NowX_OutOfRange_FlagFalse(string todayIso)
    {
        var tl = MakeTimeline(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            Lane("M1", "L", "#0078D4"));

        var vm = TimelineLayoutEngine.Build(tl, DateOnly.Parse(todayIso));

        vm.Now.InRange.Should().BeFalse();
    }

    [Fact]
    public void Build_XOf_AtStartIsZero_AtEndIsSvgWidth_AtMidpointIsHalf()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 7, 1);
        var mid = new DateOnly(2026, 4, 1);

        var lane = Lane("M1", "Lane 1", "#0078D4",
            new Milestone { Date = start, Type = MilestoneType.Poc, Label = "s" },
            new Milestone { Date = mid, Type = MilestoneType.Prod, Label = "m" },
            new Milestone { Date = end, Type = MilestoneType.Checkpoint, Label = "e" });

        var vm = TimelineLayoutEngine.Build(MakeTimeline(start, end, lane), new DateOnly(2026, 4, 1));

        var ms = vm.Lanes[0].Milestones;
        ms[0].X.Should().BeApproximately(0, 0.001);
        ms[2].X.Should().BeApproximately(TimelineLayoutEngine.SvgWidth, 0.001);
        double totalDays = (end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays;
        double expectedMid = (mid.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).TotalDays / totalDays * TimelineLayoutEngine.SvgWidth;
        ms[1].X.Should().BeApproximately(expectedMid, 0.001);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(6)]
    public void Build_LaneYs_EvenlyDistributedWithPadding(int laneCount)
    {
        var lanes = Enumerable.Range(0, laneCount)
            .Select(i => Lane($"M{i}", $"Lane {i}", "#000000"))
            .ToArray();
        var tl = MakeTimeline(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), lanes);

        var vm = TimelineLayoutEngine.Build(tl, new DateOnly(2026, 3, 1));

        vm.Lanes.Should().HaveCount(laneCount);
        foreach (var lane in vm.Lanes)
        {
            lane.Y.Should().BeGreaterThanOrEqualTo(TimelineLayoutEngine.TopPad);
            lane.Y.Should().BeLessThanOrEqualTo(TimelineLayoutEngine.SvgHeight - TimelineLayoutEngine.BottomPad);
        }

        if (laneCount > 2)
        {
            var diffs = vm.Lanes.Zip(vm.Lanes.Skip(1), (a, b) => b.Y - a.Y).ToList();
            diffs.Should().OnlyContain(d => Math.Abs(d - diffs[0]) < 0.0001);
        }
    }

    [Fact]
    public void Build_NowMarker_InRangeTrueWhenTodayWithinTimeline_FalseOtherwise()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);
        var tl = MakeTimeline(start, end, Lane("M1", "L", "#000"));

        TimelineLayoutEngine.Build(tl, start).Now.InRange.Should().BeTrue();
        TimelineLayoutEngine.Build(tl, end).Now.InRange.Should().BeTrue();
        TimelineLayoutEngine.Build(tl, new DateOnly(2026, 6, 15)).Now.InRange.Should().BeTrue();
        TimelineLayoutEngine.Build(tl, new DateOnly(2025, 12, 31)).Now.InRange.Should().BeFalse();
        TimelineLayoutEngine.Build(tl, new DateOnly(2027, 1, 1)).Now.InRange.Should().BeFalse();
    }

    [Fact]
    public void Build_MonthGridlines_OneForEachMonthBoundaryInclusive_WithInvariantAbbreviations()
    {
        var tl = MakeTimeline(new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30),
            Lane("M1", "L", "#000"));

        var vm = TimelineLayoutEngine.Build(tl, new DateOnly(2026, 3, 15));

        vm.Gridlines.Select(g => g.Label).Should().Equal("Jan", "Feb", "Mar", "Apr", "May", "Jun");
    }

    [Fact]
    public void Build_CaptionAlternation_AndOutOfRangeSkipped_AndExplicitOverrideWins()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 12, 31);

        var close1 = new DateOnly(2026, 6, 1);
        var close2 = new DateOnly(2026, 6, 3);
        var outOfRange = new DateOnly(2027, 1, 15);
        var far = new DateOnly(2026, 11, 1);

        var lane = Lane("M1", "L", "#000",
            new Milestone { Date = close1, Type = MilestoneType.Poc, Label = "a" },
            new Milestone { Date = close2, Type = MilestoneType.Prod, Label = "b" },
            new Milestone { Date = outOfRange, Type = MilestoneType.Checkpoint, Label = "skip" },
            new Milestone { Date = far, Type = MilestoneType.Checkpoint, Label = "c", CaptionPosition = CaptionPosition.Below });

        var vm = TimelineLayoutEngine.Build(MakeTimeline(start, end, lane), new DateOnly(2026, 6, 15));

        var ms = vm.Lanes[0].Milestones;
        ms.Should().HaveCount(3, "out-of-range milestone is skipped");
        ms[0].CaptionPosition.Should().Be(CaptionPlacement.Above);
        ms[1].CaptionPosition.Should().Be(CaptionPlacement.Below, "within 50px of previous -> alternate");
        ms.Last(m => m.Caption == "c").CaptionPosition.Should().Be(CaptionPlacement.Below, "explicit data overrides computed");
    }
}