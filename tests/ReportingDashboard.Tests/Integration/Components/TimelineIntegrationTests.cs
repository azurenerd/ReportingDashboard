using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Sections;
using ReportingDashboard.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests for the Timeline component verifying end-to-end rendering
/// with realistic data scenarios, multi-track interactions, and full parameter
/// contract validation. These tests render the actual component with real model
/// objects and assert on the composed SVG output.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineIntegrationTests : TestContext
{
    #region Realistic Data Helpers

    /// <summary>
    /// Creates a timeline that mirrors a real-world executive dashboard with
    /// 3 tracks, mixed milestone types, and a realistic 6-month date range.
    /// </summary>
    private static TimelineData CreateRealisticDashboardTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-10",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Name = "M1", Label = "Platform Modernization", Color = "#4285F4",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-01-15", Type = "checkpoint", Label = "Design Review" },
                    new() { Date = "2026-02-28", Type = "poc", Label = "PoC Complete" },
                    new() { Date = "2026-04-15", Type = "checkpoint", Label = "Beta" },
                    new() { Date = "2026-06-01", Type = "production", Label = "GA Release" }
                }
            },
            new()
            {
                Name = "M2", Label = "Data Pipeline v2", Color = "#34A853",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-01", Type = "poc", Label = "Spike Done" },
                    new() { Date = "2026-03-15", Type = "checkpoint", Label = "Schema Lock" },
                    new() { Date = "2026-05-30", Type = "production", Label = "Ship" }
                }
            },
            new()
            {
                Name = "M3", Label = "Auth Overhaul", Color = "#EA4335",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-03-01", Type = "dot", Label = "Kickoff" },
                    new() { Date = "2026-04-01", Type = "poc", Label = "Token PoC" },
                    new() { Date = "2026-05-15", Type = "checkpoint", Label = "Security Audit" },
                    new() { Date = "2026-06-15", Type = "production", Label = "Rollout" }
                }
            }
        }
    };

    private static TimelineData CreateMaxTrackTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-01",
        Tracks = Enumerable.Range(1, 10).Select(i => new TimelineTrack
        {
            Name = $"M{i}",
            Label = $"Workstream {i}",
            Color = $"#{(i * 25):X2}{(255 - i * 25):X2}{(i * 15):X2}",
            Milestones = new List<Milestone>
            {
                new() { Date = $"2026-{(i % 6) + 1:D2}-15", Type = i % 4 == 0 ? "poc" : i % 4 == 1 ? "production" : i % 4 == 2 ? "checkpoint" : "dot", Label = $"WS{i} Milestone" }
            }
        }).ToList()
    };

    #endregion

    #region Full Component Rendering with Realistic Data

    [Fact]
    public void Timeline_RealisticData_RendersAllThreeTracksWithLabels()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var trackLabels = cut.FindAll(".tl-track-label");
        trackLabels.Count.Should().Be(3);

        var trackIds = cut.FindAll(".tl-track-id");
        trackIds[0].TextContent.Should().Be("M1");
        trackIds[1].TextContent.Should().Be("M2");
        trackIds[2].TextContent.Should().Be("M3");

        var trackNames = cut.FindAll(".tl-track-name");
        trackNames[0].TextContent.Should().Be("Platform Modernization");
        trackNames[1].TextContent.Should().Be("Data Pipeline v2");
        trackNames[2].TextContent.Should().Be("Auth Overhaul");
    }

    [Fact]
    public void Timeline_RealisticData_RendersAllMilestoneTypes()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;

        // PoC diamonds (gold) — 3 across all tracks
        var pocPolygons = Regex.Matches(markup, @"<polygon[^>]*fill=""#F4B400""");
        pocPolygons.Count.Should().Be(3, "Should have 3 PoC milestones across all tracks");

        // Production diamonds (green) — 3 across all tracks
        var prodPolygons = Regex.Matches(markup, @"<polygon[^>]*fill=""#34A853""");
        prodPolygons.Count.Should().Be(3, "Should have 3 production milestones across all tracks");

        // Checkpoint circles (white fill) — 4 across all tracks
        var checkpoints = Regex.Matches(markup, @"<circle[^>]*fill=""white""");
        checkpoints.Count.Should().Be(4, "Should have 4 checkpoint milestones across all tracks");

        // Small dots (#999 fill) — 1 across all tracks
        var dots = Regex.Matches(markup, @"<circle[^>]*r=""4""[^>]*fill=""#999""");
        dots.Count.Should().Be(1, "Should have 1 dot milestone across all tracks");
    }

    [Fact]
    public void Timeline_RealisticData_AllTooltipsPresent()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;

        // Verify all milestone tooltips are rendered as <title> elements
        markup.Should().Contain("<title>Design Review - 2026-01-15</title>");
        markup.Should().Contain("<title>PoC Complete - 2026-02-28</title>");
        markup.Should().Contain("<title>Beta - 2026-04-15</title>");
        markup.Should().Contain("<title>GA Release - 2026-06-01</title>");
        markup.Should().Contain("<title>Spike Done - 2026-02-01</title>");
        markup.Should().Contain("<title>Schema Lock - 2026-03-15</title>");
        markup.Should().Contain("<title>Ship - 2026-05-30</title>");
        markup.Should().Contain("<title>Kickoff - 2026-03-01</title>");
        markup.Should().Contain("<title>Token PoC - 2026-04-01</title>");
        markup.Should().Contain("<title>Security Audit - 2026-05-15</title>");
        markup.Should().Contain("<title>Rollout - 2026-06-15</title>");
    }

    [Fact]
    public void Timeline_RealisticData_AllDateLabelsPresent()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;

        // All milestone labels should appear as text in the SVG
        var expectedLabels = new[]
        {
            "Design Review", "PoC Complete", "Beta", "GA Release",
            "Spike Done", "Schema Lock", "Ship",
            "Kickoff", "Token PoC", "Security Audit", "Rollout"
        };

        foreach (var label in expectedLabels)
        {
            markup.Should().Contain(label, $"Milestone label '{label}' should be rendered");
        }
    }

    #endregion

    #region Track Color Integration — Sidebar to SVG Consistency

    [Fact]
    public void Timeline_TrackColors_ConsistentBetweenSidebarAndSvgLines()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;

        // Each track color should appear in both the sidebar style and SVG stroke
        var expectedColors = new[] { "#4285F4", "#34A853", "#EA4335" };
        foreach (var color in expectedColors)
        {
            // Sidebar: style="color: {color};"
            markup.Should().Contain($"color: {color}", $"Sidebar should have color {color}");
            // SVG: stroke="{color}" stroke-width="3"
            Regex.IsMatch(markup, $@"stroke=""{Regex.Escape(color)}""[^>]*stroke-width=""3""")
                .Should().BeTrue($"SVG track line should have stroke {color}");
        }
    }

    [Fact]
    public void Timeline_CheckpointCircle_UsesTrackColor()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;

        // M1 (blue) has checkpoints — circle stroke should match track color
        markup.Should().Contain("stroke=\"#4285F4\"");
        // M2 (green) has a checkpoint
        markup.Should().Contain("stroke=\"#34A853\"");
        // M3 (red) has a checkpoint
        markup.Should().Contain("stroke=\"#EA4335\"");
    }

    #endregion

    #region NOW Line Positioning with Multiple Tracks

    [Fact]
    public void Timeline_NowLine_SpansFullSvgHeight()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // NOW line should span from y1=0 to y2=SvgHeight (185)
        var nowLineMatch = Regex.Match(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""0""\s+x2=""[^""]*""\s+y2=""185""\s+stroke=""#EA4335""");
        nowLineMatch.Success.Should().BeTrue("NOW line should span full SVG height");
    }

    [Fact]
    public void Timeline_NowLine_PositionedCorrectlyForApril10()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Jan 1 to Jul 1 = 181 days; Jan 1 to Apr 10 = 99 days
        // Expected X = (99/181) * 1560 ≈ 853.0
        var nowLineMatch = Regex.Match(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""0""\s+x2=""([^""]+)""\s+y2=""185""\s+stroke=""#EA4335""");
        nowLineMatch.Success.Should().BeTrue();

        var nowX = double.Parse(nowLineMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        nowX.Should().BeApproximately(853.0, 5.0, "NOW line X for Apr 10 in a Jan-Jul range");
    }

    [Fact]
    public void Timeline_NowLabel_PositionedAboveNowLine()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // NOW text y should be -4 (above SVG top edge)
        var nowTextMatch = Regex.Match(cut.Markup,
            @"<text\s+x=""([^""]+)""\s+y=""-4""[^>]*>NOW</text>");
        nowTextMatch.Success.Should().BeTrue("NOW label should be positioned at y=-4");

        // X of NOW text should match NOW line X
        var nowLineMatch = Regex.Match(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""0""[^>]*stroke=""#EA4335""");
        var lineX = double.Parse(nowLineMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        var textX = double.Parse(nowTextMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        textX.Should().BeApproximately(lineX, 0.01, "NOW label X should match NOW line X");
    }

    #endregion

    #region Milestone X-Position Ordering

    [Fact]
    public void Timeline_MilestonesOnSameTrack_OrderedLeftToRightByDate()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-02-01", Type = "poc", Label = "First" },
                        new() { Date = "2026-04-01", Type = "poc", Label = "Second" },
                        new() { Date = "2026-06-01", Type = "poc", Label = "Third" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Extract X positions of all PoC polygon center-X values (top point of diamond)
        var polygonMatches = Regex.Matches(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");
        polygonMatches.Count.Should().Be(3);

        var xPositions = new List<double>();
        foreach (Match m in polygonMatches)
        {
            // First point is top: "cx,cy-size ..."
            var topPointX = double.Parse(m.Groups[1].Value.Split(' ')[0].Split(',')[0], CultureInfo.InvariantCulture);
            xPositions.Add(topPointX);
        }

        xPositions[0].Should().BeLessThan(xPositions[1], "Feb milestone should be left of Apr");
        xPositions[1].Should().BeLessThan(xPositions[2], "Apr milestone should be left of Jun");
    }

    #endregion

    #region Multi-Track Y-Position Separation

    [Fact]
    public void Timeline_ThreeTracks_MilestonesAtDifferentYPositions()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Extract Y positions of horizontal track lines
        var trackLineMatches = Regex.Matches(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""([^""]+)""\s+x2=""1560""\s+y2=""([^""]+)""\s+stroke=""#[0-9A-Fa-f]+""\s+stroke-width=""3""");
        trackLineMatches.Count.Should().Be(3);

        var yPositions = trackLineMatches.Cast<Match>()
            .Select(m => double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();

        // All Y positions should be distinct
        yPositions.Distinct().Count().Should().Be(3, "Each track should have a unique Y position");

        // Y positions should be in ascending order (top to bottom)
        yPositions.Should().BeInAscendingOrder();

        // All within SVG bounds
        yPositions.Should().AllSatisfy(y =>
        {
            y.Should().BeGreaterThan(0);
            y.Should().BeLessThan(185);
        });
    }

    [Fact]
    public void Timeline_ThreeTracks_EvenlySpacedAtApprox46px()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var trackLineMatches = Regex.Matches(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""([^""]+)""\s+x2=""1560""\s+y2=""[^""]*""\s+stroke=""#[0-9A-Fa-f]+""\s+stroke-width=""3""");

        var yPositions = trackLineMatches.Cast<Match>()
            .Select(m => double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();

        // spacing = 185 / (3+1) = 46.25
        var expectedSpacing = 185.0 / 4;
        var actualSpacing = yPositions[1] - yPositions[0];
        actualSpacing.Should().BeApproximately(expectedSpacing, 0.5);

        var actualSpacing2 = yPositions[2] - yPositions[1];
        actualSpacing2.Should().BeApproximately(expectedSpacing, 0.5);
    }

    #endregion

    #region 10-Track Stress Test

    [Fact]
    public void Timeline_TenTracks_AllTracksRenderedWithLabels()
    {
        var tl = CreateMaxTrackTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        cut.FindAll(".tl-track-label").Count.Should().Be(10);
        cut.FindAll(".tl-track-id").Count.Should().Be(10);
        cut.FindAll(".tl-track-name").Count.Should().Be(10);
    }

    [Fact]
    public void Timeline_TenTracks_AllHorizontalLinesRendered()
    {
        var tl = CreateMaxTrackTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var trackLines = Regex.Matches(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""[^""]*""\s+x2=""1560""\s+y2=""[^""]*""\s+stroke=""#[0-9A-Fa-f]+""\s+stroke-width=""3""");
        trackLines.Count.Should().Be(10);
    }

    [Fact]
    public void Timeline_TenTracks_NoYPositionsOverlap()
    {
        var tl = CreateMaxTrackTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var trackLines = Regex.Matches(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""([^""]+)""\s+x2=""1560""\s+y2=""[^""]*""\s+stroke=""#[0-9A-Fa-f]+""\s+stroke-width=""3""");

        var yPositions = trackLines.Cast<Match>()
            .Select(m => double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();

        yPositions.Distinct().Count().Should().Be(10, "All 10 tracks should have unique Y positions");

        // With 10 tracks, spacing = 185/11 ≈ 16.8px — tight but all within bounds
        yPositions.Should().AllSatisfy(y =>
        {
            y.Should().BeGreaterThan(0);
            y.Should().BeLessThan(185);
        });
    }

    [Fact]
    public void Timeline_TenTracks_AllMilestonesRendered()
    {
        var tl = CreateMaxTrackTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Each of the 10 tracks has 1 milestone; count all tooltips
        var titleMatches = Regex.Matches(cut.Markup, @"<title>[^<]+</title>");
        titleMatches.Count.Should().Be(10, "Each of 10 tracks should have 1 milestone with tooltip");
    }

    #endregion

    #region Month Grid Integration

    [Fact]
    public void Timeline_SixMonthRange_RendersExpectedMonthLabels()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        foreach (var month in months)
        {
            markup.Should().Contain(month, $"Month label '{month}' should be present");
        }

        // Jul should NOT appear since endDate is Jul 1 (the boundary)
        // Actually, the grid positions might include the boundary month;
        // just verify the 6 core months are present
    }

    [Fact]
    public void Timeline_MonthGridLines_HaveCorrectStrokeAttributes()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var gridLines = Regex.Matches(cut.Markup,
            @"<line[^>]*stroke=""#bbb""[^>]*stroke-opacity=""0\.4""");
        gridLines.Count.Should().BeGreaterThan(0, "Should have month grid lines");
    }

    [Fact]
    public void Timeline_MonthGridLines_SpanFullSvgHeight()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var gridLines = Regex.Matches(cut.Markup,
            @"<line\s+x1=""[^""]*""\s+y1=""0""\s+x2=""[^""]*""\s+y2=""185""[^>]*stroke=""#bbb""");
        gridLines.Count.Should().BeGreaterThan(0, "Grid lines should span from y=0 to y=185");
    }

    #endregion

    #region SVG Structure Integrity

    [Fact]
    public void Timeline_SvgDefs_ContainsDropShadowFilter()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;
        markup.Should().Contain("<defs>");
        markup.Should().Contain("id=\"sh\"");
        markup.Should().Contain("feDropShadow");
        markup.Should().Contain("flood-opacity=\"0.3\"");
    }

    [Fact]
    public void Timeline_SvgFontFamily_SetOnSvgRoot()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var style = svg.GetAttribute("style") ?? "";
        style.Should().Contain("Segoe UI");
    }

    [Fact]
    public void Timeline_Svg_HasXmlnsAttribute()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        svg.GetAttribute("xmlns").Should().Be("http://www.w3.org/2000/svg");
    }

    #endregion

    #region Edge Case: Milestones at Date Range Boundaries

    [Fact]
    public void Timeline_MilestoneAtExactStartDate_RendersAtX0()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-01-01", Type = "poc", Label = "Day1" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");
        polygonMatch.Success.Should().BeTrue();

        var topPointX = double.Parse(
            polygonMatch.Groups[1].Value.Split(' ')[0].Split(',')[0],
            CultureInfo.InvariantCulture);
        topPointX.Should().BeApproximately(0, 1.0);
    }

    [Fact]
    public void Timeline_MilestoneAtExactEndDate_RendersAtSvgWidth()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-07-01", Type = "production", Label = "Final" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#34A853""");
        polygonMatch.Success.Should().BeTrue();

        var topPointX = double.Parse(
            polygonMatch.Groups[1].Value.Split(' ')[0].Split(',')[0],
            CultureInfo.InvariantCulture);
        topPointX.Should().BeApproximately(1560, 1.0);
    }

    [Fact]
    public void Timeline_MilestoneBeforeStartDate_ClampedToZero()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-03-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-01-01", Type = "poc", Label = "Way Early" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");
        polygonMatch.Success.Should().BeTrue();

        var topPointX = double.Parse(
            polygonMatch.Groups[1].Value.Split(' ')[0].Split(',')[0],
            CultureInfo.InvariantCulture);
        topPointX.Should().Be(0, "Out-of-range milestone should be clamped to 0");
    }

    [Fact]
    public void Timeline_MilestoneAfterEndDate_ClampedToSvgWidth()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-03-01",
            NowDate = "2026-02-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-12-01", Type = "production", Label = "Way Late" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#34A853""");
        polygonMatch.Success.Should().BeTrue();

        var topPointX = double.Parse(
            polygonMatch.Groups[1].Value.Split(' ')[0].Split(',')[0],
            CultureInfo.InvariantCulture);
        topPointX.Should().Be(1560, "Out-of-range milestone should be clamped to SVG width");
    }

    #endregion

    #region Edge Case: Mixed Valid and Invalid Milestones

    [Fact]
    public void Timeline_MixedValidAndInvalidDates_OnlyValidRendered()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "not-a-date", Type = "poc", Label = "Invalid1" },
                        new() { Date = "", Type = "production", Label = "Empty" },
                        new() { Date = "2026-04-01", Type = "poc", Label = "Valid" },
                        new() { Date = "abc123", Type = "checkpoint", Label = "Invalid2" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;
        markup.Should().Contain("Valid");
        markup.Should().NotContain("Invalid1");
        markup.Should().NotContain("Empty");
        markup.Should().NotContain("Invalid2");

        // Only 1 polygon (the valid PoC)
        var polygons = Regex.Matches(markup, @"<polygon\s");
        polygons.Count.Should().Be(1);
    }

    #endregion

    #region Edge Case: Track With No Milestones Among Others

    [Fact]
    public void Timeline_TrackWithNoMilestones_StillRendersTrackLine()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Active Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-01", Type = "poc", Label = "PoC" }
                    }
                },
                new()
                {
                    Name = "M2", Label = "Empty Track", Color = "#FF0000",
                    Milestones = new List<Milestone>()
                },
                new()
                {
                    Name = "M3", Label = "Another Active", Color = "#00FF00",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-05-01", Type = "production", Label = "Ship" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // All 3 track lines should render
        var trackLines = Regex.Matches(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""[^""]*""\s+x2=""1560""\s+y2=""[^""]*""\s+stroke=""#[0-9A-Fa-f]+""\s+stroke-width=""3""");
        trackLines.Count.Should().Be(3);

        // All 3 labels should render
        cut.FindAll(".tl-track-label").Count.Should().Be(3);

        // Empty track (red) line should exist
        cut.Markup.Should().Contain("stroke=\"#FF0000\"");
    }

    #endregion

    #region Parameter Update / Re-render

    [Fact]
    public void Timeline_ParameterUpdate_ReRendersWithNewData()
    {
        var initialData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Initial", Color = "#FF0000", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, initialData));
        cut.Find(".tl-track-name").TextContent.Should().Be("Initial");

        // Update with new data
        var updatedData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Updated", Color = "#00FF00", Milestones = new() },
                new() { Name = "M2", Label = "New Track", Color = "#0000FF", Milestones = new() }
            }
        };

        cut.SetParametersAndRender(p => p.Add(x => x.TimelineData, updatedData));

        cut.FindAll(".tl-track-label").Count.Should().Be(2);
        var names = cut.FindAll(".tl-track-name");
        names[0].TextContent.Should().Be("Updated");
        names[1].TextContent.Should().Be("New Track");

        // New colors should be in the markup
        cut.Markup.Should().Contain("#00FF00");
        cut.Markup.Should().Contain("#0000FF");
    }

    [Fact]
    public void Timeline_ParameterUpdate_NowDateChange_MovesNowLine()
    {
        var data1 = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-02-01",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, data1));

        var nowMatch1 = Regex.Match(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""0""[^>]*stroke=""#EA4335""");
        var nowX1 = double.Parse(nowMatch1.Groups[1].Value, CultureInfo.InvariantCulture);

        // Move NOW date forward
        var data2 = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-06-01",
            Tracks = new List<TimelineTrack>()
        };

        cut.SetParametersAndRender(p => p.Add(x => x.TimelineData, data2));

        var nowMatch2 = Regex.Match(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""0""[^>]*stroke=""#EA4335""");
        var nowX2 = double.Parse(nowMatch2.Groups[1].Value, CultureInfo.InvariantCulture);

        nowX2.Should().BeGreaterThan(nowX1, "Moving NOW date forward should move line right");
    }

    #endregion

    #region Full Rendering Consistency

    [Fact]
    public void Timeline_IdenticalData_ProducesIdenticalMarkup()
    {
        var tl = CreateRealisticDashboardTimeline();

        var cut1 = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        var markup1 = cut1.Markup;

        var cut2 = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        var markup2 = cut2.Markup;

        markup1.Should().Be(markup2, "Identical data should produce deterministic output");
    }

    #endregion

    #region Sidebar + SVG Layout Structure

    [Fact]
    public void Timeline_LayoutStructure_SidebarAndSvgBoxAreSiblings()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var tlArea = cut.Find(".tl-area");
        var children = tlArea.Children;

        // Should have at least sidebar and svg-box as children
        children.Length.Should().BeGreaterOrEqualTo(2);

        // First child should be sidebar, second should be svg-box
        var hasSidebar = false;
        var hasSvgBox = false;
        foreach (var child in children)
        {
            if (child.ClassList.Contains("tl-sidebar")) hasSidebar = true;
            if (child.ClassList.Contains("tl-svg-box")) hasSvgBox = true;
        }

        hasSidebar.Should().BeTrue("Should have .tl-sidebar");
        hasSvgBox.Should().BeTrue("Should have .tl-svg-box");
    }

    [Fact]
    public void Timeline_SvgIsInsideSvgBox()
    {
        var tl = CreateRealisticDashboardTimeline();
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var svgBox = cut.Find(".tl-svg-box");
        var svg = svgBox.QuerySelector("svg");
        svg.Should().NotBeNull("SVG element should be inside .tl-svg-box");
    }

    #endregion

    #region Null/Empty Data Transition

    [Fact]
    public void Timeline_TransitionFromNullToValidData_RendersCorrectly()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, (TimelineData?)null));

        // Should render shell with NOW line
        cut.Find(".tl-area").Should().NotBeNull();
        cut.Markup.Should().Contain("NOW");
        cut.FindAll(".tl-track-label").Count.Should().Be(0);

        // Now set real data
        cut.SetParametersAndRender(p =>
            p.Add(x => x.TimelineData, CreateRealisticDashboardTimeline()));

        cut.FindAll(".tl-track-label").Count.Should().Be(3);
        cut.Markup.Should().Contain("Platform Modernization");
    }

    [Fact]
    public void Timeline_TransitionFromValidDataToEmpty_ClearsTracksKeepsNow()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateRealisticDashboardTimeline()));

        cut.FindAll(".tl-track-label").Count.Should().Be(3);

        // Clear to empty
        var emptyData = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>()
        };

        cut.SetParametersAndRender(p => p.Add(x => x.TimelineData, emptyData));

        cut.FindAll(".tl-track-label").Count.Should().Be(0);
        cut.FindAll("polygon").Count.Should().Be(0);
        cut.Markup.Should().Contain("NOW", "NOW line should persist even with no tracks");
    }

    #endregion

    #region Date Range Variations

    [Fact]
    public void Timeline_NarrowDateRange_OneMonth_StillRenders()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-03-01",
            EndDate = "2026-04-01",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Sprint", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-15", Type = "poc", Label = "Mid" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        cut.Find(".tl-area").Should().NotBeNull();
        cut.Markup.Should().Contain("#F4B400"); // PoC diamond rendered

        // NOW at midpoint should be near center
        var nowMatch = Regex.Match(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""0""[^>]*stroke=""#EA4335""");
        var nowX = double.Parse(nowMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        // 15/31 * 1560 ≈ 755
        nowX.Should().BeInRange(700, 810);
    }

    [Fact]
    public void Timeline_WideYearLongRange_StillRenders()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2027-01-01",
            NowDate = "2026-07-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Annual", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-07-01", Type = "production", Label = "Mid Year" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        cut.Find(".tl-area").Should().NotBeNull();

        // Milestone at midpoint should be near center
        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#34A853""");
        var topPointX = double.Parse(
            polygonMatch.Groups[1].Value.Split(' ')[0].Split(',')[0],
            CultureInfo.InvariantCulture);
        // 181/365 * 1560 ≈ 773 (Jul 1 in a full year)
        topPointX.Should().BeInRange(750, 800);
    }

    #endregion
}