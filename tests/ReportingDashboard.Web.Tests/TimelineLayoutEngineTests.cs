using Xunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests;

public class TimelineLayoutEngineTests
{
    private static Timeline CreateTimeline(
        DateOnly? start = null, DateOnly? end = null,
        IReadOnlyList<TimelineLane>? lanes = null)
    {
        return new Timeline
        {
            Start = start ?? new DateOnly(2026, 1, 1),
            End = end ?? new DateOnly(2026, 6, 30),
            Lanes = lanes ?? new[]
            {
                new TimelineLane
                {
                    Id = "M1", Label = "Lane 1", Color = "#0078D4",
                    Milestones = Array.Empty<Milestone>()
                }
            }
        };
    }

    [Fact]
    public void XOf_StartDate_Returns_Zero()
    {
        var timeline = CreateTimeline();
        var today = new DateOnly(2026, 3, 15);
        var vm = TimelineLayoutEngine.Build(timeline, today);

        vm.Gridlines.Should().NotBeEmpty();
        // Jan gridline should be at x=0 (start of timeline)
        vm.Gridlines[0].X.Should().BeApproximately(0, 0.01);
        vm.Gridlines[0].Label.Should().Be("Jan");
    }

    [Fact]
    public void NowMarker_InRange_When_Today_Between_Start_And_End()
    {
        var timeline = CreateTimeline();
        var today = new DateOnly(2026, 4, 15);
        var vm = TimelineLayoutEngine.Build(timeline, today);

        vm.Now.InRange.Should().BeTrue();
        vm.Now.X.Should().BeGreaterThan(0);
        vm.Now.X.Should().BeLessThan(TimelineLayoutEngine.SvgWidth);
    }

    [Fact]
    public void NowMarker_OutOfRange_When_Today_After_End()
    {
        var timeline = CreateTimeline();
        var today = new DateOnly(2026, 8, 1);
        var vm = TimelineLayoutEngine.Build(timeline, today);

        vm.Now.InRange.Should().BeFalse();
    }

    [Fact]
    public void LaneY_Distribution_With_Three_Lanes()
    {
        var lanes = Enumerable.Range(1, 3).Select(i => new TimelineLane
        {
            Id = $"M{i}", Label = $"Lane {i}", Color = "#000000",
            Milestones = Array.Empty<Milestone>()
        }).ToArray();

        var timeline = CreateTimeline(lanes: lanes);
        var vm = TimelineLayoutEngine.Build(timeline, new DateOnly(2026, 3, 1));

        vm.Lanes.Should().HaveCount(3);
        // Lanes should be evenly spaced in the available area
        var y0 = vm.Lanes[0].Y;
        var y1 = vm.Lanes[1].Y;
        var y2 = vm.Lanes[2].Y;
        var spacing = y1 - y0;
        (y2 - y1).Should().BeApproximately(spacing, 0.01);
    }

    [Fact]
    public void Milestone_X_Matches_Expected_Position()
    {
        var milestone = new Milestone
        {
            Date = new DateOnly(2026, 4, 1),
            Type = MilestoneType.Poc,
            Label = "Test"
        };

        var lanes = new[]
        {
            new TimelineLane
            {
                Id = "M1", Label = "Lane 1", Color = "#0078D4",
                Milestones = new[] { milestone }
            }
        };

        var timeline = CreateTimeline(lanes: lanes);
        var vm = TimelineLayoutEngine.Build(timeline, new DateOnly(2026, 1, 1));

        var totalDays = (double)(timeline.End.DayNumber - timeline.Start.DayNumber);
        var milestoneDays = (double)(milestone.Date.DayNumber - timeline.Start.DayNumber);
        var expectedX = milestoneDays / totalDays * TimelineLayoutEngine.SvgWidth;

        vm.Lanes[0].Milestones[0].X.Should().BeApproximately(expectedX, 0.01);
    }

    [Fact]
    public void Empty_Timeline_Returns_Empty_ViewModel()
    {
        var timeline = CreateTimeline(
            start: new DateOnly(2026, 6, 30),
            end: new DateOnly(2026, 1, 1));
        var vm = TimelineLayoutEngine.Build(timeline, new DateOnly(2026, 3, 1));

        vm.Should().Be(TimelineViewModel.Empty);
    }
}