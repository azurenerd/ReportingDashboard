using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class TimelineRenderingIntegrationTests : TestContext
{
    private static TimelineData CreateTimelineWithAllMilestoneTypes() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-06-30",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Id = "M1",
                Name = "Feature Alpha",
                Color = "#0078D4",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-01-20", Label = "Jan 20", Type = "checkpoint" },
                    new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" },
                    new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" }
                }
            },
            new()
            {
                Id = "M2",
                Name = "Feature Beta",
                Color = "#00897B",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-03-10", Label = "Mar 10", Type = "poc" }
                }
            },
            new()
            {
                Id = "M3",
                Name = "Feature Gamma",
                Color = "#546E7A",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-05-01", Label = "May 1", Type = "production" }
                }
            }
        }
    };

    [Fact]
    public void Timeline_WithThreeTracks_RendersThreeTrackLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        cut.FindAll(".tl-label").Should().HaveCount(3);
    }

    [Fact]
    public void Timeline_TrackIdColors_MatchTrackColorProperty()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        var ids = cut.FindAll(".tl-id");
        ids[0].GetAttribute("style").Should().Contain("#0078D4");
        ids[1].GetAttribute("style").Should().Contain("#00897B");
        ids[2].GetAttribute("style").Should().Contain("#546E7A");
    }

    [Fact]
    public void Timeline_SvgHasCorrectWidthAttribute()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
    }

    [Fact]
    public void Timeline_SvgHeight_AdjustsForTrackCount()
    {
        var timeline = CreateTimelineWithAllMilestoneTypes();
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        // firstTrackY(42) + (trackCount-1)*trackSpacing(56) + 30
        // 42 + 2*56 + 30 = 184
        height.Should().Be(184);
    }

    [Fact]
    public void Timeline_SingleTrack_SvgHeightIsMinimal()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Only", Color = "#000" }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        // 42 + 0*56 + 30 = 72
        height.Should().Be(72);
    }

    [Fact]
    public void Timeline_RendersDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        cut.Markup.Should().Contain("<filter id=\"shadow\"");
        cut.Markup.Should().Contain("feDropShadow");
    }

    [Fact]
    public void Timeline_PocMilestones_RenderAsGoldPolygons()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        var polygons = cut.FindAll("polygon[fill='#F4B400']");
        polygons.Should().HaveCount(2); // M1 and M2 each have a poc
    }

    [Fact]
    public void Timeline_ProductionMilestones_RenderAsGreenPolygons()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        var polygons = cut.FindAll("polygon[fill='#34A853']");
        polygons.Should().HaveCount(2); // M1 and M3 each have a production
    }

    [Fact]
    public void Timeline_CheckpointMilestones_RenderAsCircles()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        var circles = cut.FindAll("circle");
        circles.Should().HaveCount(1); // Only M1 has a checkpoint
    }

    [Fact]
    public void Timeline_NowLine_RenderedWithCorrectAttributes()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        cut.Markup.Should().Contain("stroke=\"#EA4335\"");
        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public void Timeline_NowOutsideRange_NoNowLineRendered()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2027-01-01", // after end
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
    public void Timeline_MonthLabels_RenderedInSvg()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        cut.Markup.Should().Contain("Jan");
        cut.Markup.Should().Contain("Feb");
        cut.Markup.Should().Contain("Mar");
        cut.Markup.Should().Contain("Apr");
        cut.Markup.Should().Contain("May");
        cut.Markup.Should().Contain("Jun");
    }

    [Fact]
    public void Timeline_MilestoneLabels_RenderedAsTextElements()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        cut.Markup.Should().Contain("Jan 20");
        cut.Markup.Should().Contain("Feb 15");
        cut.Markup.Should().Contain("Apr 1");
        cut.Markup.Should().Contain("Mar 10");
        cut.Markup.Should().Contain("May 1");
    }

    [Fact]
    public void Timeline_MilestoneTooltips_RenderedAsTitleElements()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, CreateTimelineWithAllMilestoneTypes()));

        var titles = cut.FindAll("title");
        titles.Should().HaveCount(5); // 3 milestones in M1 + 1 in M2 + 1 in M3
    }

    [Fact]
    public void Timeline_WithNullModel_RendersNothing()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, (TimelineData?)null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_EmptyTracks_RendersSvgAndLabelsContainerButNoTrackElements()
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
        cut.Find(".tl-svg-box").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().BeEmpty();
    }

    [Fact]
    public void Timeline_FiveTracks_SvgHeightScalesCorrectly()
    {
        var tracks = Enumerable.Range(1, 5).Select(i => new TimelineTrack
        {
            Id = $"M{i}",
            Name = $"Track {i}",
            Color = $"#00{i:D2}FF"
        }).ToList();

        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = tracks
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        // 42 + 4*56 + 30 = 296
        height.Should().Be(296);
    }
}