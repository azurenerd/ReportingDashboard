using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class TimelineComponentTests : TestContext
{
    private static TimelineData CreateValidTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-06-30",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Id = "M1",
                Name = "Chatbot",
                Color = "#0078D4",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" },
                    new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" },
                    new() { Date = "2026-01-15", Label = "Jan 15", Type = "checkpoint" }
                }
            },
            new()
            {
                Id = "M2",
                Name = "Pipeline",
                Color = "#00897B",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-03-01", Label = "Mar 1", Type = "checkpoint" }
                }
            }
        }
    };

    [Fact]
    public void Timeline_WithNullModel_RendersNothing()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, (TimelineData?)null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_WithValidModel_RendersSvgElement()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_WithValidModel_RendersTrackLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.FindAll(".tl-label").Should().HaveCount(2);
        cut.FindAll(".tl-id").Should().HaveCount(2);
        cut.FindAll(".tl-name").Should().HaveCount(2);
    }

    [Fact]
    public void Timeline_WithValidModel_RendersTrackNames()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("Chatbot");
        cut.Markup.Should().Contain("Pipeline");
    }

    [Fact]
    public void Timeline_WithValidModel_RendersTrackIds()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("M2");
    }

    [Fact]
    public void Timeline_WithValidModel_RendersNowLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("#EA4335");
    }

    [Fact]
    public void Timeline_WithPocMilestone_RendersGoldDiamond()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("#F4B400");
    }

    [Fact]
    public void Timeline_WithProductionMilestone_RendersGreenDiamond()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("#34A853");
    }

    [Fact]
    public void Timeline_WithCheckpointMilestone_RendersCircle()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.FindAll("circle").Should().NotBeEmpty();
    }

    [Fact]
    public void Timeline_SvgContainsDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("filter");
        cut.Markup.Should().Contain("shadow");
    }

    [Fact]
    public void Timeline_WithEmptyTracks_RendersLabelsContainer()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Find(".tl-labels").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().BeEmpty();
    }

    [Fact]
    public void Timeline_RendersSvgBox()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Find(".tl-svg-box").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_RendersTrackHorizontalLines()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        // Each track gets a horizontal line
        var lines = cut.FindAll("line");
        lines.Should().NotBeEmpty();
    }

    [Fact]
    public void Timeline_TrackColorsApplied()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("#0078D4");
        cut.Markup.Should().Contain("#00897B");
    }

    [Fact]
    public void Timeline_MilestoneLabelText_Rendered()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        cut.Markup.Should().Contain("Feb 15");
        cut.Markup.Should().Contain("Apr 1");
        cut.Markup.Should().Contain("Jan 15");
        cut.Markup.Should().Contain("Mar 1");
    }

    [Fact]
    public void Timeline_NowDateOutsideRange_DoesNotRenderNowLine()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2025-01-01", // before start
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track", Color = "#000" }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().NotContain("NOW");
    }

    [Fact]
    public void Timeline_SingleTrack_SvgHasCorrectStructure()
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
                    Color = "#FF0000",
                    Milestones = new List<MilestoneMarker>()
                }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().HaveCount(1);
    }

    [Fact]
    public void Timeline_MilestonesTitles_RenderedForTooltips()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateValidTimeline()));

        var titles = cut.FindAll("title");
        titles.Should().NotBeEmpty();
    }
}