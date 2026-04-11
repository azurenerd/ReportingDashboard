using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class TimelineTests : TestContext
{
    private static TimelineData CreateBasicTimeline(int trackCount = 3)
    {
        var tracks = new List<TimelineTrack>();
        var colors = new[] { "#0078D4", "#00897B", "#546E7A", "#E91E63", "#FF9800" };
        for (int i = 0; i < trackCount; i++)
        {
            tracks.Add(new TimelineTrack
            {
                Id = $"M{i + 1}",
                Name = $"Track {i + 1}",
                Color = colors[i % colors.Length],
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-03-15", Label = "PoC", Type = "poc" },
                    new() { Date = "2026-05-01", Label = "GA", Type = "production" },
                    new() { Date = "2026-04-01", Label = "Review", Type = "checkpoint" }
                }
            });
        }

        return new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = tracks
        };
    }

    [Fact]
    public void Timeline_NullModel_RendersNothing()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_ValidModel_RendersSidebarLabels()
    {
        var timeline = CreateBasicTimeline(3);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var labels = cut.FindAll(".tl-label");
        labels.Should().HaveCount(3);
    }

    [Fact]
    public void Timeline_TrackIds_DisplayInSidebar()
    {
        var timeline = CreateBasicTimeline(3);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var ids = cut.FindAll(".tl-id");
        ids.Should().HaveCount(3);
        ids[0].TextContent.Should().Be("M1");
        ids[1].TextContent.Should().Be("M2");
        ids[2].TextContent.Should().Be("M3");
    }

    [Fact]
    public void Timeline_TrackNames_DisplayInSidebar()
    {
        var timeline = CreateBasicTimeline(2);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var names = cut.FindAll(".tl-name");
        names.Should().HaveCount(2);
        names[0].TextContent.Should().Be("Track 1");
        names[1].TextContent.Should().Be("Track 2");
    }

    [Fact]
    public void Timeline_TrackIdColors_MatchTrackColor()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Track 1", Color = "#FF0000" }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var idSpan = cut.Find(".tl-id");
        idSpan.GetAttribute("style").Should().Contain("color:#FF0000");
    }

    [Fact]
    public void Timeline_SvgElement_RendersWithCorrectWidth()
    {
        var timeline = CreateBasicTimeline(3);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
    }

    [Fact]
    public void Timeline_SvgElement_HasOverflowVisible()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        svg.GetAttribute("overflow").Should().Be("visible");
    }

    [Fact]
    public void Timeline_SvgDefs_ContainsDropShadowFilter()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var filter = cut.Find("filter");
        filter.GetAttribute("id").Should().Be("shadow");
        cut.Markup.Should().Contain("feDropShadow");
    }

    [Fact]
    public void Timeline_MonthGridLines_AreRendered()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Month grid lines should have stroke="#bbb"
        var markup = cut.Markup;
        markup.Should().Contain("stroke=\"#bbb\"");
        markup.Should().Contain("stroke-opacity=\"0.4\"");
    }

    [Fact]
    public void Timeline_MonthLabels_AreRendered()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        // Should contain month abbreviations for Jan-Jun range
        markup.Should().Contain("Jan");
        markup.Should().Contain("Feb");
        markup.Should().Contain("Mar");
        markup.Should().Contain("Apr");
        markup.Should().Contain("May");
        markup.Should().Contain("Jun");
    }

    [Fact]
    public void Timeline_NowLine_IsRendered()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain("stroke-width=\"2\"");
        markup.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void Timeline_NowLabel_IsRendered()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        markup.Should().Contain("NOW");
        markup.Should().Contain("font-weight=\"700\"");
    }

    [Fact]
    public void Timeline_NowLineOutOfRange_IsNotRendered()
    {
        var timeline = CreateBasicTimeline(1);
        timeline.NowDate = "2025-06-01"; // before start date

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        // Should not contain NOW label when date is out of range
        markup.Should().NotContain(">NOW<");
    }

    [Fact]
    public void Timeline_PocMilestone_RendersGoldDiamond()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "PoC Done", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygon = cut.Find("polygon");
        polygon.GetAttribute("fill").Should().Be("#F4B400");
        polygon.GetAttribute("filter").Should().Be("url(#shadow)");
    }

    [Fact]
    public void Timeline_ProductionMilestone_RendersGreenDiamond()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-05-01", Label = "GA Release", Type = "production" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygon = cut.Find("polygon");
        polygon.GetAttribute("fill").Should().Be("#34A853");
        polygon.GetAttribute("filter").Should().Be("url(#shadow)");
    }

    [Fact]
    public void Timeline_CheckpointMilestone_RendersCircle()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-04-01", Label = "Checkpoint", Type = "checkpoint" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var circle = cut.Find("circle");
        circle.GetAttribute("fill").Should().Be("white");
        circle.GetAttribute("stroke").Should().Be("#0078D4");
        circle.GetAttribute("stroke-width").Should().Be("2.5");
        circle.GetAttribute("r").Should().Be("5");
    }

    [Fact]
    public void Timeline_MilestoneTooltips_ContainLabels()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "My PoC Label", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var title = cut.Find("title");
        title.TextContent.Should().Be("My PoC Label");
    }

    [Fact]
    public void Timeline_TrackLines_AreRendered()
    {
        var timeline = CreateBasicTimeline(2);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        // Track lines have stroke-width="3"
        markup.Should().Contain("stroke-width=\"3\"");
        markup.Should().Contain("stroke=\"#0078D4\"");
        markup.Should().Contain("stroke=\"#00897B\"");
    }

    [Fact]
    public void Timeline_SingleTrack_RendersCorrectly()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var labels = cut.FindAll(".tl-label");
        labels.Should().HaveCount(1);

        var svg = cut.Find("svg");
        svg.Should().NotBeNull();
    }

    [Fact]
    public void Timeline_FiveTracks_AllRender()
    {
        var timeline = CreateBasicTimeline(5);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var labels = cut.FindAll(".tl-label");
        labels.Should().HaveCount(5);
    }

    [Fact]
    public void Timeline_EmptyMilestonesOnTrack_NoMarkersButTrackLineRenders()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Empty Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>()
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Track label renders
        cut.Find(".tl-id").TextContent.Should().Be("M1");
        // SVG renders with no polygons or circles
        cut.FindAll("polygon").Should().BeEmpty();
        cut.FindAll("circle").Should().BeEmpty();
    }

    [Fact]
    public void Timeline_EmptyTracks_RendersSvgButNoTrackLines()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Should still render SVG container
        cut.Find("svg").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().BeEmpty();
    }

    [Fact]
    public void Timeline_MilestoneLabels_AreRenderedAsText()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Alpha Release", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("Alpha Release");
    }

    [Fact]
    public void Timeline_MilestoneTextLabels_HaveMiddleAnchor()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Test Label", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("text-anchor=\"middle\"");
    }

    [Fact]
    public void Timeline_SvgContainer_HasCorrectClass()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Find(".tl-svg-box").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_LabelsContainer_HasCorrectClass()
    {
        var timeline = CreateBasicTimeline(1);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Find(".tl-labels").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_MixedMilestoneTypes_AllRenderCorrectShapes()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Mixed", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-01", Label = "PoC", Type = "poc" },
                        new() { Date = "2026-04-01", Label = "Check", Type = "checkpoint" },
                        new() { Date = "2026-05-15", Label = "Prod", Type = "production" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // 2 polygons (poc + production), 1 circle (checkpoint)
        cut.FindAll("polygon").Should().HaveCount(2);
        cut.FindAll("circle").Should().HaveCount(1);
    }

    [Fact]
    public void Timeline_NowDateAtStart_RendersNowLine()
    {
        var timeline = CreateBasicTimeline(1);
        timeline.NowDate = "2026-01-01"; // at start

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain(">NOW<");
    }

    [Fact]
    public void Timeline_NowDateAtEnd_RendersNowLine()
    {
        var timeline = CreateBasicTimeline(1);
        timeline.NowDate = "2026-06-30"; // at end

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain(">NOW<");
    }

    [Fact]
    public void Timeline_MultipleTracks_DifferentColors_AllRendered()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "A", Color = "#FF0000", Milestones = new() },
                new() { Id = "M2", Name = "B", Color = "#00FF00", Milestones = new() },
                new() { Id = "M3", Name = "C", Color = "#0000FF", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        markup.Should().Contain("#FF0000");
        markup.Should().Contain("#00FF00");
        markup.Should().Contain("#0000FF");
    }

    [Fact]
    public void Timeline_SvgHeight_DynamicBasedOnTrackCount()
    {
        // With 1 track: firstTrackY(42) + (1-1)*56 + 30 = 72
        var timeline1 = CreateBasicTimeline(1);
        var cut1 = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline1));
        var svg1 = cut1.Find("svg");
        int.TryParse(svg1.GetAttribute("height"), out var h1).Should().BeTrue();
        h1.Should().Be(72); // 42 + 0*56 + 30

        // With 3 tracks: 42 + 2*56 + 30 = 184
        var timeline3 = CreateBasicTimeline(3);
        var cut3 = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline3));
        var svg3 = cut3.Find("svg");
        int.TryParse(svg3.GetAttribute("height"), out var h3).Should().BeTrue();
        h3.Should().Be(184); // 42 + 2*56 + 30
    }

    [Fact]
    public void Timeline_TrackHorizontalLines_SpanFullSvgWidth()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Test", Color = "#0078D4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Track line should go from x1=0 to x2=1560
        cut.Markup.Should().Contain("x1=\"0\"");
        cut.Markup.Should().Contain("x2=\"1560\"");
    }

    [Fact]
    public void Timeline_DiamondPolygon_HasFourPoints()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "PoC", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygon = cut.Find("polygon");
        var points = polygon.GetAttribute("points");
        points.Should().NotBeNullOrEmpty();
        // Diamond has 4 vertices = 4 space-separated coordinate pairs
        var pairs = points!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        pairs.Should().HaveCount(4);
    }

    [Fact]
    public void Timeline_UnknownMilestoneType_RendersAsCheckpointCircle()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Test", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Unknown", Type = "something_else" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Falls through to else branch which renders circle
        cut.FindAll("circle").Should().HaveCount(1);
        cut.FindAll("polygon").Should().BeEmpty();
    }
}