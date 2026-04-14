using Bunit;
using Xunit;
using ReportingDashboard.Models;
using ReportingDashboard.Components;

namespace ReportingDashboard.UITests.Tests;

public class DashboardHappyPathTests : TestContext
{
    private static TimelineData CreateTestTimelineData() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-06-30",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Name = "M1",
                Label = "Chatbot & MS Role",
                Color = "#0078D4",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-01-12", Type = "checkpoint", Label = "Jan 12" },
                    new() { Date = "2026-03-26", Type = "poc", Label = "Mar 26 PoC" },
                    new() { Date = "2026-05-01", Type = "production", Label = "May Prod" }
                }
            },
            new()
            {
                Name = "M2",
                Label = "Compliance Reporting",
                Color = "#00897B",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-15", Type = "checkpoint", Label = "Feb 15" },
                    new() { Date = "2026-04-20", Type = "poc", Label = "Apr PoC" }
                }
            },
            new()
            {
                Name = "M3",
                Label = "Data Pipeline v2",
                Color = "#546E7A",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-03-01", Type = "checkpoint", Label = "Mar 1" },
                    new() { Date = "2026-06-15", Type = "production", Label = "Jun Prod" }
                }
            }
        }
    };

    // ────────────────────────────────────────────
    // Timeline Area rendering
    // ────────────────────────────────────────────

    [Fact]
    public void Timeline_RendersTimelineAreaContainer()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var area = cut.Find(".tl-area");
        Assert.NotNull(area);
    }

    [Fact]
    public void Timeline_RendersSidebar_WithAllTrackLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var sidebar = cut.Find(".tl-sidebar");
        Assert.Contains("M1", sidebar.InnerHtml);
        Assert.Contains("M2", sidebar.InnerHtml);
        Assert.Contains("M3", sidebar.InnerHtml);
        Assert.Contains("Chatbot", sidebar.InnerHtml);
        Assert.Contains("Compliance Reporting", sidebar.InnerHtml);
        Assert.Contains("Data Pipeline v2", sidebar.InnerHtml);
    }

    [Fact]
    public void Timeline_RendersSvg_WithCorrectWidth()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

    [Fact]
    public void Timeline_RendersSvg_WithMinHeight185()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        Assert.True(height >= 185, $"SVG height should be >= 185 but was {height}");
    }

    [Fact]
    public void Timeline_RendersDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var markup = cut.Markup;
        Assert.Contains("filter", markup);
        Assert.Contains("id=\"sh\"", markup);
        Assert.Contains("feDropShadow", markup);
    }

    [Fact]
    public void Timeline_RendersMonthGridLines()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        // Jan-Jun = 6 months, so 6 vertical grid lines
        var gridLines = cut.FindAll("line[stroke='#bbb']");
        Assert.Equal(6, gridLines.Count);
    }

    [Fact]
    public void Timeline_RendersMonthLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var markup = cut.Markup;
        Assert.Contains("Jan", markup);
        Assert.Contains("Feb", markup);
        Assert.Contains("Mar", markup);
        Assert.Contains("Apr", markup);
        Assert.Contains("May", markup);
        Assert.Contains("Jun", markup);
    }

    [Fact]
    public void Timeline_RendersNowDashedLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var nowLines = cut.FindAll("line[stroke='#EA4335']");
        Assert.Single(nowLines);

        var nowLine = nowLines[0];
        Assert.Equal("5,3", nowLine.GetAttribute("stroke-dasharray"));
        Assert.Equal("2", nowLine.GetAttribute("stroke-width"));
    }

    [Fact]
    public void Timeline_RendersNowLabel()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var markup = cut.Markup;
        Assert.Contains(">NOW<", markup);
    }

    [Fact]
    public void Timeline_RendersHorizontalTrackLines()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var trackLines = cut.FindAll("line[stroke-width='3']");
        Assert.Equal(3, trackLines.Count);
    }

    [Fact]
    public void Timeline_RendersTrackLinesWithCorrectColors()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var markup = cut.Markup;
        Assert.Contains("stroke=\"#0078D4\"", markup);
        Assert.Contains("stroke=\"#00897B\"", markup);
        Assert.Contains("stroke=\"#546E7A\"", markup);
    }

    [Fact]
    public void Timeline_RendersCheckpointCircles()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        // M1: 1 checkpoint, M2: 1 checkpoint, M3: 1 checkpoint = 3 circles
        var circles = cut.FindAll("circle");
        Assert.Equal(3, circles.Count);

        foreach (var circle in circles)
        {
            Assert.Equal("white", circle.GetAttribute("fill"));
            Assert.Equal("2.5", circle.GetAttribute("stroke-width"));
            Assert.Equal("7", circle.GetAttribute("r"));
        }
    }

    [Fact]
    public void Timeline_RendersPocDiamonds_WithGoldFill()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var polygons = cut.FindAll("polygon[fill='#F4B400']");
        // M1: Mar 26 PoC, M2: Apr PoC = 2 poc diamonds
        Assert.Equal(2, polygons.Count);

        foreach (var polygon in polygons)
        {
            Assert.Equal("url(#sh)", polygon.GetAttribute("filter"));
        }
    }

    [Fact]
    public void Timeline_RendersProductionDiamonds_WithGreenFill()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var polygons = cut.FindAll("polygon[fill='#34A853']");
        // M1: May Prod, M3: Jun Prod = 2 production diamonds
        Assert.Equal(2, polygons.Count);

        foreach (var polygon in polygons)
        {
            Assert.Equal("url(#sh)", polygon.GetAttribute("filter"));
        }
    }

    [Fact]
    public void Timeline_RendersMilestoneTooltips_ViaTitleElement()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var titles = cut.FindAll("title");
        Assert.True(titles.Count > 0, "Should have <title> elements for tooltips");

        var markup = cut.Markup;
        Assert.Contains("Jan 12", markup);
        Assert.Contains("Mar 26 PoC", markup);
        Assert.Contains("May Prod", markup);
        Assert.Contains("Feb 15", markup);
        Assert.Contains("Apr PoC", markup);
        Assert.Contains("Jun Prod", markup);
    }

    [Fact]
    public void Timeline_RendersMilestoneDateLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var textEls = cut.FindAll("text[text-anchor='middle']");
        Assert.True(textEls.Count >= 7, $"Expected >=7 milestone labels, found {textEls.Count}");
    }

    [Fact]
    public void Timeline_NowLinePosition_IsBetweenStartAndEnd()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTestTimelineData()));

        var nowLine = cut.Find("line[stroke='#EA4335']");
        var x1 = double.Parse(nowLine.GetAttribute("x1")!,
            System.Globalization.CultureInfo.InvariantCulture);

        Assert.True(x1 > 0, "NOW line x should be > 0");
        Assert.True(x1 < 1560, "NOW line x should be < 1560");
        Assert.True(x1 > 700 && x1 < 1000,
            $"NOW line x={x1} should be roughly between 700 and 1000 for Apr 10");
    }

    // ────────────────────────────────────────────
    // Dynamic track count
    // ────────────────────────────────────────────

    [Fact]
    public void Timeline_SingleTrack_RendersCorrectly()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Solo Track", Color = "#0078D4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-04-01", Type = "poc", Label = "Apr PoC" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        cut.Find(".tl-area");
        cut.Find("svg");
        Assert.Single(cut.FindAll("line[stroke-width='3']"));
        Assert.Single(cut.FindAll("polygon[fill='#F4B400']"));
    }

    [Fact]
    public void Timeline_FiveTracks_GrowsSvgHeight()
    {
        var tracks = Enumerable.Range(1, 5).Select(i => new TimelineTrack
        {
            Name = $"M{i}", Label = $"Track {i}", Color = "#0078D4",
            Milestones = new List<Milestone>
            {
                new() { Date = $"2026-0{i}-15", Type = "checkpoint", Label = $"Check {i}" }
            }
        }).ToList();

        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-03-15",
            Tracks = tracks
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        var svg = cut.Find("svg");
        var height = int.Parse(svg.GetAttribute("height")!);
        Assert.True(height >= 280, $"SVG height should grow for 5 tracks but was {height}");
    }

    // ────────────────────────────────────────────
    // Edge cases & error handling
    // ────────────────────────────────────────────

    [Fact]
    public void Timeline_EmptyTracks_RendersGracefully()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        var area = cut.Find(".tl-area");
        Assert.NotNull(area);
        Assert.Contains("No timeline tracks configured", cut.Markup);
    }

    [Fact]
    public void Timeline_InvalidDates_ShowsFallbackMessage()
    {
        var data = new TimelineData
        {
            StartDate = "not-a-date",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#000" }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        Assert.Contains("Timeline data is not available", cut.Markup);
    }

    [Fact]
    public void Timeline_DefaultTimelineData_ShowsFallbackMessage()
    {
        var cut = RenderComponent<Timeline>();

        var area = cut.Find(".tl-area");
        Assert.NotNull(area);
        Assert.Contains("Timeline data is not available", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackWithNoMilestones_RendersTrackLineOnly()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Empty Track", Color = "#0078D4",
                    Milestones = new List<Milestone>()
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        Assert.Single(cut.FindAll("line[stroke-width='3']"));
        Assert.Empty(cut.FindAll("circle"));
        Assert.Empty(cut.FindAll("polygon"));
    }

    [Fact]
    public void Timeline_MilestoneWithInvalidDate_IsSkipped()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#0078D4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "bad-date", Type = "poc", Label = "Bad" },
                        new() { Date = "2026-03-15", Type = "poc", Label = "Good PoC" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        var pocDiamonds = cut.FindAll("polygon[fill='#F4B400']");
        Assert.Single(pocDiamonds);
        Assert.Contains("Good PoC", cut.Markup);
    }

    [Fact]
    public void Timeline_DiamondPointsFormat_UsesInvariantCulture()
    {
        var data = CreateTestTimelineData();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data));

        var polygons = cut.FindAll("polygon");
        foreach (var polygon in polygons)
        {
            var points = polygon.GetAttribute("points");
            Assert.NotNull(points);
            Assert.DoesNotContain(",,", points);
            Assert.Matches(@"[\d.]+,[\d.]+ [\d.]+,[\d.]+ [\d.]+,[\d.]+ [\d.]+,[\d.]+", points);
        }
    }
}