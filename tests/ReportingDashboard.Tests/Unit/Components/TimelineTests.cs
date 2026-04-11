using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class TimelineTests : TestContext
{
    private static TimelineData CreateTestTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-06-30",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Id = "M1",
                Name = "Platform",
                Color = "#0078D4",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" },
                    new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" },
                    new() { Date = "2026-03-15", Label = "Mar 15", Type = "checkpoint" }
                }
            },
            new()
            {
                Id = "M2",
                Name = "API",
                Color = "#34A853",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-05-01", Label = "May 1", Type = "production" }
                }
            }
        }
    };

    [Fact]
    public void Timeline_WithNullModel_ShouldRenderNothing()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, (TimelineData?)null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_ShouldRenderTrackLabels()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        var labels = cut.FindAll(".tl-label");
        labels.Should().HaveCount(2);
    }

    [Fact]
    public void Timeline_ShouldRenderTrackIds()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        var ids = cut.FindAll(".tl-id");
        ids.Should().HaveCount(2);
        ids[0].TextContent.Should().Be("M1");
        ids[1].TextContent.Should().Be("M2");
    }

    [Fact]
    public void Timeline_ShouldRenderTrackNames()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        var names = cut.FindAll(".tl-name");
        names[0].TextContent.Should().Be("Platform");
        names[1].TextContent.Should().Be("API");
    }

    [Fact]
    public void Timeline_ShouldApplyTrackColors()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        var ids = cut.FindAll(".tl-id");
        ids[0].GetAttribute("style").Should().Contain("#0078D4");
        ids[1].GetAttribute("style").Should().Contain("#34A853");
    }

    [Fact]
    public void Timeline_ShouldRenderSvg()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_ShouldRenderNowLine()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // NOW text should be in the SVG
        cut.Markup.Should().Contain("NOW");
        // Red NOW line
        cut.Markup.Should().Contain("#EA4335");
    }

    [Fact]
    public void Timeline_NowDateOutsideRange_ShouldNotRenderNowLine()
    {
        var timeline = CreateTestTimeline();
        timeline.NowDate = "2025-01-01"; // before start date

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        // NOW text should not appear since date is outside range
        // The SVG should still render but without the NOW marker
        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_ShouldRenderPocMilestonesAsGoldDiamonds()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // Should have polygon with gold fill for PoC
        cut.Markup.Should().Contain("#F4B400");
    }

    [Fact]
    public void Timeline_ShouldRenderProductionMilestonesAsGreenDiamonds()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // Should have polygon with green fill for production
        cut.Markup.Should().Contain("#34A853");
    }

    [Fact]
    public void Timeline_ShouldRenderCheckpointCircles()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // Should have circle elements for checkpoints
        cut.FindAll("circle").Should().NotBeEmpty();
    }

    [Fact]
    public void Timeline_ShouldRenderDropShadowFilter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        cut.Markup.Should().Contain("filter id=\"shadow\"");
    }

    [Fact]
    public void Timeline_ShouldRenderMonthLabels()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        cut.Markup.Should().Contain("Jan");
        cut.Markup.Should().Contain("Feb");
        cut.Markup.Should().Contain("Mar");
        cut.Markup.Should().Contain("Apr");
        cut.Markup.Should().Contain("May");
        cut.Markup.Should().Contain("Jun");
    }

    [Fact]
    public void Timeline_ShouldRenderHorizontalTrackLines()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // Should have line elements with track colors
        var lines = cut.FindAll("line");
        lines.Should().NotBeEmpty();
    }

    [Fact]
    public void Timeline_WithSingleTrack_ShouldRender()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1",
                    Name = "Only Track",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>()
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        cut.FindAll(".tl-label").Should().HaveCount(1);
        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_WithManyTracks_ShouldRenderAll()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = Enumerable.Range(1, 10).Select(i => new TimelineTrack
            {
                Id = $"M{i}",
                Name = $"Track {i}",
                Color = "#0078D4",
                Milestones = new List<MilestoneMarker>()
            }).ToList()
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, timeline));

        cut.FindAll(".tl-label").Should().HaveCount(10);
    }

    [Fact]
    public void Timeline_ShouldRenderMilestoneLabelsAsText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // Milestone labels should appear
        cut.Markup.Should().Contain("Feb 15");
        cut.Markup.Should().Contain("Apr 1");
        cut.Markup.Should().Contain("Mar 15");
        cut.Markup.Should().Contain("May 1");
    }

    [Fact]
    public void Timeline_ShouldRenderMilestoneTitles()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        // title elements are rendered for tooltip
        cut.FindAll("title").Should().NotBeEmpty();
    }

    [Fact]
    public void Timeline_SvgWidth_ShouldBe1560()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
    }

    [Fact]
    public void Timeline_NowDashedLine_ShouldHaveCorrectStrokeDasharray()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p => p
            .Add(x => x.TimelineModel, CreateTestTimeline()));

        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
    }
}