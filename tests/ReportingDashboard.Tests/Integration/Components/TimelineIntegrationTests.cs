using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class TimelineIntegrationTests : TestContext
{
    private static TimelineData CreateThreeTrackTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-06-30",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Name = "M1", Label = "Chatbot", Color = "#0078D4",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-01", Type = "checkpoint", Label = "Design" },
                    new() { Date = "2026-03-15", Type = "poc", Label = "PoC Complete" },
                    new() { Date = "2026-05-01", Type = "production", Label = "GA Release" }
                }
            },
            new()
            {
                Name = "M2", Label = "Data Pipeline", Color = "#00897B",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-01-15", Type = "checkpoint", Label = "Kickoff" },
                    new() { Date = "2026-04-01", Type = "poc", Label = "Pipeline PoC" }
                }
            },
            new()
            {
                Name = "M3", Label = "Compliance", Color = "#546E7A",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-06-01", Type = "production", Label = "Audit" }
                }
            }
        }
    };

    [Fact]
    public void Timeline_ThreeTracks_RendersAllTrackNames()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("M2");
        cut.Markup.Should().Contain("M3");
    }

    [Fact]
    public void Timeline_ThreeTracks_RendersAllTrackLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        cut.Markup.Should().Contain("Chatbot");
        cut.Markup.Should().Contain("Data Pipeline");
        cut.Markup.Should().Contain("Compliance");
    }

    [Fact]
    public void Timeline_ThreeTracks_RendersAllTrackColors()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        cut.Markup.Should().Contain("#0078D4");
        cut.Markup.Should().Contain("#00897B");
        cut.Markup.Should().Contain("#546E7A");
    }

    [Fact]
    public void Timeline_RendersPocMilestones_AsGoldPolygons()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var polygons = cut.FindAll("polygon[fill='#F4B400']");
        polygons.Should().HaveCount(2); // M1 PoC + M2 Pipeline PoC
    }

    [Fact]
    public void Timeline_RendersProductionMilestones_AsGreenPolygons()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var polygons = cut.FindAll("polygon[fill='#34A853']");
        polygons.Should().HaveCount(2); // M1 GA + M3 Audit
    }

    [Fact]
    public void Timeline_RendersCheckpoints_AsCircles()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var circles = cut.FindAll("circle");
        circles.Should().HaveCount(2); // M1 Design + M2 Kickoff
    }

    [Fact]
    public void Timeline_RendersNowLine_WithRedDashedStroke()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        cut.Markup.Should().Contain("stroke=\"#EA4335\"");
        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public void Timeline_RendersDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        cut.Find("defs").Should().NotBeNull();
        cut.Find("filter").Should().NotBeNull();
        cut.Markup.Should().Contain("url(#sh)");
    }

    [Fact]
    public void Timeline_SvgHeight_AdjustsForTrackCount()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        // 3 tracks * 56 = 168, but min is 185
        height.Should().BeGreaterThanOrEqualTo(185);
    }

    [Fact]
    public void Timeline_ManyTracks_IncreaseSvgHeight()
    {
        var tracks = Enumerable.Range(1, 8).Select(i => new TimelineTrack
        {
            Name = $"M{i}", Label = $"Track {i}", Color = "#000",
            Milestones = new List<Milestone>()
        }).ToList();

        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = tracks
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        // 8 * 56 = 448 > 185
        height.Should().BeGreaterThanOrEqualTo(448);
    }

    [Fact]
    public void Timeline_NullData_RendersNothing()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, (TimelineData?)null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_EmptyTracks_RendersNothing()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_MilestoneLabels_AreRenderedAsTextElements()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var textElements = cut.FindAll("text");
        textElements.Should().NotBeEmpty();

        var allText = string.Join(" ", textElements.Select(t => t.TextContent));
        allText.Should().Contain("PoC Complete");
        allText.Should().Contain("GA Release");
        allText.Should().Contain("Kickoff");
    }

    [Fact]
    public void Timeline_MilestoneLabels_HaveTitleTooltips()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var titles = cut.FindAll("title");
        titles.Should().NotBeEmpty();

        var tooltips = titles.Select(t => t.TextContent).ToList();
        tooltips.Should().Contain("Design");
        tooltips.Should().Contain("PoC Complete");
        tooltips.Should().Contain("GA Release");
    }

    [Fact]
    public void Timeline_MonthLabels_AreRenderedInSvg()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateThreeTrackTimeline()));

        var textElements = cut.FindAll("text");
        var allText = string.Join(" ", textElements.Select(t => t.TextContent));

        // StartDate is 2026-01-01, so first month label should be "Jan"
        allText.Should().Contain("Jan");
    }

    [Fact]
    public void Timeline_HorizontalTrackLines_MatchTrackCount()
    {
        var timeline = CreateThreeTrackTimeline();

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, timeline));

        // Lines include: 6 grid lines + 3 track lines + 1 NOW line = 10
        var lines = cut.FindAll("line");
        lines.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void Timeline_TrackWithNullMilestones_DoesNotThrow()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#000", Milestones = null! }
            }
        };

        var action = () => RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        action.Should().NotThrow();
    }
}