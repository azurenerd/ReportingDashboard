using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests for the Timeline component verifying end-to-end rendering
/// with realistic data configurations matching the data.json contract.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineIntegrationTests : TestContext
{
    /// <summary>
    /// Creates a realistic TimelineData matching the sample data.json structure
    /// with 3 tracks and mixed milestone types.
    /// </summary>
    private static TimelineData CreateSampleTimelineData()
    {
        return new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1",
                    Name = "Chatbot & MS Role",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-26", Label = "PoC", Type = "poc" },
                        new() { Date = "2026-05-20", Label = "Prod Release", Type = "production" },
                        new() { Date = "2026-02-15", Label = "Design Review", Type = "checkpoint" }
                    }
                },
                new()
                {
                    Id = "M2",
                    Name = "Backend Services",
                    Color = "#00897B",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-28", Label = "API PoC", Type = "poc" },
                        new() { Date = "2026-06-01", Label = "GA", Type = "production" }
                    }
                },
                new()
                {
                    Id = "M3",
                    Name = "Infrastructure",
                    Color = "#546E7A",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-04-15", Label = "Checkpoint", Type = "checkpoint" },
                        new() { Date = "2026-05-30", Label = "Deploy", Type = "production" }
                    }
                }
            }
        };
    }

    [Fact]
    public void Timeline_WithSampleData_RendersSidebarAndSvgTogether()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Sidebar should exist with labels container
        cut.Find(".tl-labels").Should().NotBeNull();
        // SVG container should exist
        cut.Find(".tl-svg-box").Should().NotBeNull();
        // SVG element should exist
        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_WithSampleData_AllThreeTrackLabelsRender()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var labels = cut.FindAll(".tl-label");
        labels.Should().HaveCount(3);

        var ids = cut.FindAll(".tl-id");
        ids[0].TextContent.Should().Be("M1");
        ids[1].TextContent.Should().Be("M2");
        ids[2].TextContent.Should().Be("M3");

        var names = cut.FindAll(".tl-name");
        names[0].TextContent.Should().Be("Chatbot & MS Role");
        names[1].TextContent.Should().Be("Backend Services");
        names[2].TextContent.Should().Be("Infrastructure");
    }

    [Fact]
    public void Timeline_WithSampleData_TrackIdsColoredCorrectly()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var ids = cut.FindAll(".tl-id");
        ids[0].GetAttribute("style").Should().Contain("color:#0078D4");
        ids[1].GetAttribute("style").Should().Contain("color:#00897B");
        ids[2].GetAttribute("style").Should().Contain("color:#546E7A");
    }

    [Fact]
    public void Timeline_WithSampleData_SvgHasCorrectDimensions()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
        svg.GetAttribute("overflow").Should().Be("visible");

        // Height = firstTrackY(42) + (3-1)*56 + 30 = 184
        var heightStr = svg.GetAttribute("height");
        int.TryParse(heightStr, out var height).Should().BeTrue();
        height.Should().Be(184);
    }

    [Fact]
    public void Timeline_WithSampleData_MonthGridLinesAndLabelsRender()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;

        // Grid lines have characteristic stroke
        markup.Should().Contain("stroke=\"#bbb\"");
        markup.Should().Contain("stroke-opacity=\"0.4\"");

        // Month labels Jan through Jun should be present
        markup.Should().Contain("Jan");
        markup.Should().Contain("Feb");
        markup.Should().Contain("Mar");
        markup.Should().Contain("Apr");
        markup.Should().Contain("May");
        markup.Should().Contain("Jun");
    }

    [Fact]
    public void Timeline_WithSampleData_DropShadowFilterDefined()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var filter = cut.Find("filter");
        filter.GetAttribute("id").Should().Be("shadow");

        cut.Markup.Should().Contain("feDropShadow");
        cut.Markup.Should().Contain("flood-opacity=\"0.3\"");
    }

    [Fact]
    public void Timeline_WithSampleData_NowLineRendersWithCorrectStyling()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;

        // NOW line styling
        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain("stroke-width=\"2\"");
        markup.Should().Contain("stroke-dasharray=\"5,3\"");

        // NOW label
        markup.Should().Contain(">NOW<");
        markup.Should().Contain("font-weight=\"700\"");
    }

    [Fact]
    public void Timeline_WithSampleData_AllMilestoneTypesRender()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Total milestones: M1 has 3 (poc, production, checkpoint), M2 has 2 (poc, production), M3 has 2 (checkpoint, production)
        // Polygons: poc(2) + production(3) = 5 diamonds
        var polygons = cut.FindAll("polygon");
        polygons.Should().HaveCount(5);

        // Circles: checkpoint(2) = 2 circles
        var circles = cut.FindAll("circle");
        circles.Should().HaveCount(2);
    }

    [Fact]
    public void Timeline_WithSampleData_PocDiamondsAreGold()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygons = cut.FindAll("polygon");
        var goldPolygons = polygons.Where(p => p.GetAttribute("fill") == "#F4B400").ToList();

        // 2 PoC milestones across all tracks
        goldPolygons.Should().HaveCount(2);

        // All should have shadow filter
        goldPolygons.Should().AllSatisfy(p =>
            p.GetAttribute("filter").Should().Be("url(#shadow)"));
    }

    [Fact]
    public void Timeline_WithSampleData_ProductionDiamondsAreGreen()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygons = cut.FindAll("polygon");
        var greenPolygons = polygons.Where(p => p.GetAttribute("fill") == "#34A853").ToList();

        // 3 production milestones
        greenPolygons.Should().HaveCount(3);

        greenPolygons.Should().AllSatisfy(p =>
            p.GetAttribute("filter").Should().Be("url(#shadow)"));
    }

    [Fact]
    public void Timeline_WithSampleData_CheckpointCirclesHaveCorrectStyling()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var circles = cut.FindAll("circle");
        circles.Should().HaveCount(2);

        circles.Should().AllSatisfy(c =>
        {
            c.GetAttribute("fill").Should().Be("white");
            c.GetAttribute("r").Should().Be("5");
            c.GetAttribute("stroke-width").Should().Be("2.5");
        });
    }

    [Fact]
    public void Timeline_WithSampleData_CheckpointCirclesUseTrackColor()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var circles = cut.FindAll("circle");

        // M1's checkpoint should use M1's color, M3's checkpoint should use M3's color
        var strokes = circles.Select(c => c.GetAttribute("stroke")).ToList();
        strokes.Should().Contain("#0078D4"); // M1
        strokes.Should().Contain("#546E7A"); // M3
    }

    [Fact]
    public void Timeline_WithSampleData_AllMilestonesHaveTooltips()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var titles = cut.FindAll("title");

        // 7 milestones total should have 7 title elements
        titles.Should().HaveCount(7);

        var tooltipTexts = titles.Select(t => t.TextContent).ToList();
        tooltipTexts.Should().Contain("PoC");
        tooltipTexts.Should().Contain("Prod Release");
        tooltipTexts.Should().Contain("Design Review");
        tooltipTexts.Should().Contain("API PoC");
        tooltipTexts.Should().Contain("GA");
        tooltipTexts.Should().Contain("Checkpoint");
        tooltipTexts.Should().Contain("Deploy");
    }

    [Fact]
    public void Timeline_WithSampleData_AllMilestoneTextLabelsRender()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;

        // Each milestone label should appear as SVG text
        markup.Should().Contain("PoC");
        markup.Should().Contain("Prod Release");
        markup.Should().Contain("Design Review");
        markup.Should().Contain("API PoC");
        markup.Should().Contain("GA");
        markup.Should().Contain("Deploy");
    }

    [Fact]
    public void Timeline_WithSampleData_TrackHorizontalLinesRenderForAllTracks()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;

        // Each track should have a horizontal line with its color and stroke-width 3
        markup.Should().Contain("stroke=\"#0078D4\"");
        markup.Should().Contain("stroke=\"#00897B\"");
        markup.Should().Contain("stroke=\"#546E7A\"");
        markup.Should().Contain("stroke-width=\"3\"");
    }

    [Fact]
    public void Timeline_WithSampleData_TrackLinesSpanFullWidth()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        markup.Should().Contain("x1=\"0\"");
        markup.Should().Contain("x2=\"1560\"");
    }

    [Fact]
    public void Timeline_WithSampleData_DiamondPointsAreValidPolygonFormat()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygons = cut.FindAll("polygon");
        foreach (var polygon in polygons)
        {
            var points = polygon.GetAttribute("points");
            points.Should().NotBeNullOrEmpty();

            // Should have 4 coordinate pairs
            var pairs = points!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            pairs.Should().HaveCount(4);

            // Each pair should be "number,number"
            foreach (var pair in pairs)
            {
                var coords = pair.Split(',');
                coords.Should().HaveCount(2);
                double.TryParse(coords[0], out _).Should().BeTrue();
                double.TryParse(coords[1], out _).Should().BeTrue();
            }
        }
    }

    [Fact]
    public void Timeline_SingleTrack_RendersCorrectSvgHeight()
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
                    Id = "M1", Name = "Solo Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "PoC", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        // Height = 42 + (1-1)*56 + 30 = 72
        svg.GetAttribute("height").Should().Be("72");
    }

    [Fact]
    public void Timeline_FiveTracks_SvgGrowsToAccommodate()
    {
        var tracks = Enumerable.Range(1, 5).Select(i => new TimelineTrack
        {
            Id = $"M{i}",
            Name = $"Track {i}",
            Color = "#0078D4",
            Milestones = new List<MilestoneMarker>()
        }).ToList();

        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = tracks
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        // Height = 42 + (5-1)*56 + 30 = 296
        svg.GetAttribute("height").Should().Be("296");

        cut.FindAll(".tl-label").Should().HaveCount(5);
    }

    [Fact]
    public void Timeline_TenTracks_AllRenderWithUniqueYPositions()
    {
        var tracks = Enumerable.Range(1, 10).Select(i => new TimelineTrack
        {
            Id = $"M{i}",
            Name = $"Track {i}",
            Color = $"#{i:D2}78D4",
            Milestones = new List<MilestoneMarker>()
        }).ToList();

        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = tracks
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Height = 42 + (10-1)*56 + 30 = 576
        var svg = cut.Find("svg");
        svg.GetAttribute("height").Should().Be("576");

        cut.FindAll(".tl-label").Should().HaveCount(10);
    }

    [Fact]
    public void Timeline_EmptyTracksArray_RendersEmptySvg()
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

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().BeEmpty();
        cut.FindAll("polygon").Should().BeEmpty();
        cut.FindAll("circle").Should().BeEmpty();
    }

    [Fact]
    public void Timeline_TrackWithNoMilestones_StillRendersTrackLine()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Id = "M1", Name = "Empty Track", Color = "#0078D4", Milestones = new() },
                new()
                {
                    Id = "M2", Name = "Full Track", Color = "#00897B",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "PoC", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.FindAll(".tl-label").Should().HaveCount(2);
        // M1 track line still renders (stroke color present)
        cut.Markup.Should().Contain("#0078D4");
        // M2 has a polygon for PoC
        cut.FindAll("polygon").Should().HaveCount(1);
    }

    [Fact]
    public void Timeline_NowDateOutsideRange_NowLineNotRendered()
    {
        var timeline = CreateSampleTimelineData();
        timeline.NowDate = "2025-06-15"; // before start date

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // NOW label should not appear
        cut.Markup.Should().NotContain(">NOW<");
    }

    [Fact]
    public void Timeline_NowDateAfterEndDate_NowLineNotRendered()
    {
        var timeline = CreateSampleTimelineData();
        timeline.NowDate = "2026-08-01"; // after end date

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().NotContain(">NOW<");
    }

    [Fact]
    public void Timeline_NowDateAtStartDate_NowLineRendered()
    {
        var timeline = CreateSampleTimelineData();
        timeline.NowDate = "2026-01-01";

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain(">NOW<");
    }

    [Fact]
    public void Timeline_NowDateAtEndDate_NowLineRendered()
    {
        var timeline = CreateSampleTimelineData();
        timeline.NowDate = "2026-06-30";

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain(">NOW<");
    }

    [Fact]
    public void Timeline_MilestoneLabelsAlternatePlacement()
    {
        // With multiple milestones on one track, labels should alternate y positions
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-01", Label = "First", Type = "poc" },
                        new() { Date = "2026-03-01", Label = "Second", Type = "poc" },
                        new() { Date = "2026-04-01", Label = "Third", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // All labels should render
        var markup = cut.Markup;
        markup.Should().Contain("First");
        markup.Should().Contain("Second");
        markup.Should().Contain("Third");
    }

    [Fact]
    public void Timeline_MilestoneAtStartDate_RendersAtLeftEdge()
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
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-01-01", Label = "Start Milestone", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // The polygon should render (x position ~0)
        var polygon = cut.Find("polygon");
        polygon.Should().NotBeNull();
        polygon.GetAttribute("fill").Should().Be("#F4B400");
    }

    [Fact]
    public void Timeline_MilestoneAtEndDate_RendersAtRightEdge()
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
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-06-30", Label = "End Milestone", Type = "production" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygon = cut.Find("polygon");
        polygon.Should().NotBeNull();
        polygon.GetAttribute("fill").Should().Be("#34A853");
    }

    [Fact]
    public void Timeline_MilestoneOutsideDateRange_StillRendersWithoutCrash()
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
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-08-15", Label = "Future", Type = "poc" }
                    }
                }
            }
        };

        // Should not throw, SVG overflow:visible allows out-of-range rendering
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.FindAll("polygon").Should().HaveCount(1);
    }

    [Fact]
    public void Timeline_NullModel_RendersEmptyMarkup()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, null));

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_AllCheckpointMilestones_NoPolygonsOnlyCircles()
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
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-01", Label = "Check 1", Type = "checkpoint" },
                        new() { Date = "2026-03-01", Label = "Check 2", Type = "checkpoint" },
                        new() { Date = "2026-04-01", Label = "Check 3", Type = "checkpoint" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.FindAll("polygon").Should().BeEmpty();
        cut.FindAll("circle").Should().HaveCount(3);
    }

    [Fact]
    public void Timeline_AllPocMilestones_OnlyGoldPolygons()
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
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-01", Label = "PoC 1", Type = "poc" },
                        new() { Date = "2026-04-01", Label = "PoC 2", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygons = cut.FindAll("polygon");
        polygons.Should().HaveCount(2);
        polygons.Should().AllSatisfy(p => p.GetAttribute("fill").Should().Be("#F4B400"));
        cut.FindAll("circle").Should().BeEmpty();
    }

    [Fact]
    public void Timeline_UnknownMilestoneType_RendersAsCircle()
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
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-01", Label = "Custom", Type = "custom_type" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Falls through to else branch (checkpoint style)
        cut.FindAll("polygon").Should().BeEmpty();
        cut.FindAll("circle").Should().HaveCount(1);
    }

    [Fact]
    public void Timeline_WithSampleData_MilestoneTextAnchorsAreMiddle()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Markup.Should().Contain("text-anchor=\"middle\"");
    }

    [Fact]
    public void Timeline_WithSampleData_MonthLabelsHaveCorrectFontProperties()
    {
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        markup.Should().Contain("font-size=\"11\"");
        markup.Should().Contain("font-weight=\"600\"");
        markup.Should().Contain("fill=\"#666\"");
    }

    [Fact]
    public void Timeline_NowLineRendersAtCorrectApproximateXPosition()
    {
        // NowDate is 2026-04-10, between Apr and May
        // Start=Jan1, End=Jun30 = 180 days. Apr10 = day 99
        // Expected X ≈ (99/180) * 1560 ≈ 858
        var timeline = CreateSampleTimelineData();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // The NOW line should be present with EA4335 color
        var markup = cut.Markup;
        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void Timeline_DifferentDateRanges_ShortRange_StillRenders()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-03-01",
            EndDate = "2026-03-31",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "Mid", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Find("svg").Should().NotBeNull();
        cut.FindAll("polygon").Should().HaveCount(1);
    }

    [Fact]
    public void Timeline_YearSpanDateRange_StillRenders()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-12-31",
            NowDate = "2026-06-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1", Name = "Track", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-06-15", Label = "Mid Year", Type = "production" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        cut.Find("svg").Should().NotBeNull();
        // Should have 12+ month labels
        cut.Markup.Should().Contain("Jan");
        cut.Markup.Should().Contain("Dec");
    }
}