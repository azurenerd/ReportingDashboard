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
                Name = "M1",
                Label = "Chatbot",
                Color = "#0078D4",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-01", Type = "checkpoint", Label = "Design" },
                    new() { Date = "2026-03-15", Type = "poc", Label = "PoC Complete" },
                    new() { Date = "2026-05-01", Type = "production", Label = "GA Release" }
                }
            },
            new()
            {
                Name = "M2",
                Label = "Data Pipeline",
                Color = "#00897B",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-01-15", Type = "checkpoint", Label = "Kickoff" }
                }
            }
        }
    };

    [Fact]
    public void NullTimelineData_RendersNothing()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, (TimelineData?)null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void EmptyTracks_RendersNothing()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, data));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void ValidData_RendersTimelineArea()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void ValidData_RendersSvgElement()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void ValidData_RendersSvgBox()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Find(".tl-svg-box").Should().NotBeNull();
    }

    [Fact]
    public void ValidData_RendersTrackNames()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("M2");
    }

    [Fact]
    public void ValidData_RendersTrackLabels()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("Chatbot");
        cut.Markup.Should().Contain("Data Pipeline");
    }

    [Fact]
    public void ValidData_RendersNowLine()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("#EA4335");
    }

    [Fact]
    public void ValidData_RendersPocMilestone_WithGoldColor()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("#F4B400");
    }

    [Fact]
    public void ValidData_RendersProductionMilestone_WithGreenColor()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("#34A853");
    }

    [Fact]
    public void ValidData_RendersCheckpointMilestone_AsCircle()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        var circles = cut.FindAll("circle");
        circles.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidData_RendersPocMilestone_AsPolygon()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        var polygons = cut.FindAll("polygon");
        polygons.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidData_RendersDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Find("filter").Should().NotBeNull();
        cut.Markup.Should().Contain("id=\"sh\"");
    }

    [Fact]
    public void ValidData_RendersHorizontalTrackLines()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        var lines = cut.FindAll("line");
        lines.Should().HaveCountGreaterThan(2); // grid lines + track lines + NOW line
    }

    [Fact]
    public void ValidData_RendersNowDashedLine()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("stroke-dasharray");
        cut.Markup.Should().Contain("5,3");
    }

    [Fact]
    public void SingleTrack_RendersCorrectly()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Only Track", Color = "#FF0000", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, data));

        cut.Markup.Should().Contain("Only Track");
        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void MilestoneLabels_AreRenderedInSvg()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        var textElements = cut.FindAll("text");
        textElements.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackColors_AreApplied()
    {
        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, CreateValidTimeline()));

        cut.Markup.Should().Contain("#0078D4");
        cut.Markup.Should().Contain("#00897B");
    }

    [Fact]
    public void NullTracks_RendersNothing()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = null!
        };

        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, data));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void TrackWithNoMilestones_StillRendersTrackLine()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Empty Track", Color = "#333", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, data));

        cut.Markup.Should().Contain("Empty Track");
        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void ManyTracks_AdjustsSvgHeight()
    {
        var tracks = Enumerable.Range(1, 6).Select(i => new TimelineTrack
        {
            Name = $"M{i}",
            Label = $"Track {i}",
            Color = "#000",
            Milestones = new()
        }).ToList();

        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = tracks
        };

        var cut = RenderComponent<Timeline>(parameters =>
            parameters.Add(p => p.TimelineData, data));

        // 6 tracks * 56 = 336 > 185 default
        var svg = cut.Find("svg");
        var heightAttr = svg.GetAttribute("height");
        int.Parse(heightAttr!).Should().BeGreaterThanOrEqualTo(336);
    }
}