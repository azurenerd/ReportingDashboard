using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class TimelineTests : TestContext
{
    private static TimelineData CreateBasicTimeline(int trackCount = 1) => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-10",
        Tracks = Enumerable.Range(1, trackCount).Select(i => new TimelineTrack
        {
            Name = $"M{i}",
            Label = $"Track {i}",
            Color = $"#{i:D6}",
            Milestones = new List<Milestone>
            {
                new() { Date = "2026-02-15", Type = "poc", Label = "Feb 15" },
                new() { Date = "2026-05-01", Type = "production", Label = "May 1" },
                new() { Date = "2026-03-15", Type = "checkpoint", Label = "Mar 15" }
            }
        }).ToList()
    };

    [Fact]
    public void Timeline_RendersSvgBox()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.NotNull(cut.Find(".tl-svg-box"));
    }

    [Fact]
    public void Timeline_RendersTrackName()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("M1", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersTrackLabel()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Track 1", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackNameHasColor()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("#4285F4", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersSvgElement()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void Timeline_SvgHasWidth1560()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

    [Fact]
    public void Timeline_SvgContainsNowLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgContainsPocDiamond()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgContainsProductionDiamond()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgContainsCheckpointCircle()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("circle", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgContainsDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("filter", cut.Markup);
        Assert.Contains("feDropShadow", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgContainsMonthLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Feb", cut.Markup);
        Assert.Contains("Mar", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasDashedStroke()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_MultipleTracks_RendersAllNames()
    {
        var tl = CreateBasicTimeline(5);
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        for (int i = 1; i <= 5; i++)
        {
            Assert.Contains($"M{i}", cut.Markup);
            Assert.Contains($"Track {i}", cut.Markup);
        }
    }

    [Fact]
    public void Timeline_SvgHeight_MinimumIs185()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Solo", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = svg.GetAttribute("height");
        Assert.Equal("185.0", height);
    }

    [Fact]
    public void Timeline_ManyTracks_SvgHeightScales()
    {
        var tl = CreateBasicTimeline(5);
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!);
        Assert.True(height >= 280);
    }

    [Fact]
    public void Timeline_TrackLines_RenderedForEachTrack()
    {
        var tl = CreateBasicTimeline(3);
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;
        var count = markup.Split("stroke-width=\"3\"").Length - 1;
        Assert.Equal(3, count);
    }
}