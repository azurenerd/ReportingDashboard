using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Sections;
using ReportingDashboard.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class TimelineTests : TestContext
{
    #region Test Data Helpers

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

    private static TimelineData CreateTimelineWithMilestoneTypes() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-01",
        Tracks = new List<TimelineTrack>
        {
            new()
            {
                Name = "M1", Label = "Track 1", Color = "#4285F4",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-01", Type = "poc", Label = "PoC Done" },
                    new() { Date = "2026-03-01", Type = "production", Label = "Go Live" },
                    new() { Date = "2026-04-01", Type = "checkpoint", Label = "Review" },
                    new() { Date = "2026-05-01", Type = "dot", Label = "Minor" }
                }
            }
        }
    };

    private static TimelineData CreateEmptyTimeline() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-01",
        Tracks = new List<TimelineTrack>()
    };

    private static TimelineData CreateTimelineWithEmptyMilestones() => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-01",
        Tracks = new List<TimelineTrack>
        {
            new() { Name = "M1", Label = "Empty Track", Color = "#FF0000", Milestones = new List<Milestone>() }
        }
    };

    #endregion

    #region Layout Structure Tests

    [Fact]
    public void Timeline_RendersTimelineAreaContainer()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_RendersSidebar()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Find(".tl-sidebar").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_RendersSvgBox()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Find(".tl-svg-box").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_RendersSvgElement()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        svg.Should().NotBeNull();
    }

    [Fact]
    public void Timeline_SvgWidthIs1560()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
    }

    [Fact]
    public void Timeline_SvgHeightIs185()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        svg.GetAttribute("height").Should().Be("185");
    }

    [Fact]
    public void Timeline_SvgHasOverflowVisible()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        svg.GetAttribute("overflow").Should().Be("visible");
    }

    #endregion

    #region Track Label Sidebar Tests

    [Fact]
    public void Timeline_RendersSingleTrackLabel()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline(1)));

        var labels = cut.FindAll(".tl-track-label");
        labels.Count.Should().Be(1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Timeline_RendersCorrectNumberOfTrackLabels(int trackCount)
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline(trackCount)));

        var labels = cut.FindAll(".tl-track-label");
        labels.Count.Should().Be(trackCount);
    }

    [Fact]
    public void Timeline_TrackIdDisplaysCorrectName()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var trackId = cut.Find(".tl-track-id");
        trackId.TextContent.Should().Be("M1");
    }

    [Fact]
    public void Timeline_TrackNameDisplaysCorrectLabel()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var trackName = cut.Find(".tl-track-name");
        trackName.TextContent.Should().Be("Track 1");
    }

    [Fact]
    public void Timeline_TrackIdHasCorrectColor()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var trackId = cut.Find(".tl-track-id");
        trackId.GetAttribute("style").Should().Contain("#4285F4");
    }

    [Fact]
    public void Timeline_MultipleTracksHaveDistinctColors()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Track 1", Color = "#FF0000", Milestones = new() },
                new() { Name = "M2", Label = "Track 2", Color = "#00FF00", Milestones = new() },
                new() { Name = "M3", Label = "Track 3", Color = "#0000FF", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var trackIds = cut.FindAll(".tl-track-id");
        trackIds[0].GetAttribute("style").Should().Contain("#FF0000");
        trackIds[1].GetAttribute("style").Should().Contain("#00FF00");
        trackIds[2].GetAttribute("style").Should().Contain("#0000FF");
    }

    [Fact]
    public void Timeline_NoTracksRendersEmptySidebar()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateEmptyTimeline()));

        var labels = cut.FindAll(".tl-track-label");
        labels.Count.Should().Be(0);
    }

    #endregion

    #region SVG Drop Shadow Filter Tests

    [Fact]
    public void Timeline_SvgContainsDropShadowFilterDef()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("feDropShadow");
    }

    [Fact]
    public void Timeline_DropShadowFilterHasCorrectId()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("id=\"sh\"");
    }

    [Fact]
    public void Timeline_DropShadowHasCorrectAttributes()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var markup = cut.Markup;
        markup.Should().Contain("dx=\"0\"");
        markup.Should().Contain("dy=\"1\"");
        markup.Should().Contain("stdDeviation=\"1.5\"");
        markup.Should().Contain("flood-opacity=\"0.3\"");
    }

    #endregion

    #region Month Grid Tests

    [Fact]
    public void Timeline_RendersMonthGridLines()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        // The SVG should contain vertical grid lines (stroke="#bbb")
        cut.Markup.Should().Contain("stroke=\"#bbb\"");
        cut.Markup.Should().Contain("stroke-opacity=\"0.4\"");
    }

    [Fact]
    public void Timeline_RendersMonthLabels()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        // Jan-Jun date range should render month labels
        var markup = cut.Markup;
        markup.Should().Contain("Jan");
        markup.Should().Contain("Feb");
        markup.Should().Contain("Mar");
        markup.Should().Contain("Apr");
        markup.Should().Contain("May");
        markup.Should().Contain("Jun");
    }

    [Fact]
    public void Timeline_MonthLabelsHaveCorrectFontAttributes()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var markup = cut.Markup;
        markup.Should().Contain("font-size=\"11\"");
        markup.Should().Contain("font-weight=\"600\"");
        markup.Should().Contain("fill=\"#666\"");
    }

    #endregion

    #region Horizontal Track Line Tests

    [Fact]
    public void Timeline_RendersHorizontalTrackLines()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Track", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Track line should use the track's color and stroke-width 3
        cut.Markup.Should().Contain("stroke=\"#4285F4\"");
        cut.Markup.Should().Contain("stroke-width=\"3\"");
    }

    [Fact]
    public void Timeline_TrackLinesSpanFullSvgWidth()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Track", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Track line should start at x1=0 and end at x2=1560
        cut.Markup.Should().Contain("x1=\"0\"");
        cut.Markup.Should().Contain("x2=\"1560\"");
    }

    [Fact]
    public void Timeline_TrackWithEmptyMilestones_StillRendersTrackLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithEmptyMilestones()));

        // Should have track line but no polygons or milestone circles
        cut.Markup.Should().Contain("stroke=\"#FF0000\"");
        cut.Markup.Should().Contain("stroke-width=\"3\"");
    }

    #endregion

    #region DateToX Interpolation Tests (via rendered output)

    [Fact]
    public void Timeline_NowLineAtStartDate_RendersAtXZero()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-01-01",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // NOW line x1 and x2 should be 0 (or very close to it)
        var nowLineMatch = Regex.Match(cut.Markup,
            @"stroke=""#EA4335""[^>]*stroke-dasharray=""5,3""");
        nowLineMatch.Success.Should().BeTrue("NOW line should exist");

        // Extract the x position of the NOW line
        var lineMatches = Regex.Matches(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""[^""]*""\s+x2=""([^""]+)""\s+y2=""[^""]*""\s+stroke=""#EA4335""");
        lineMatches.Count.Should().BeGreaterThan(0);
        var x1 = double.Parse(lineMatches[0].Groups[1].Value, CultureInfo.InvariantCulture);
        x1.Should().BeApproximately(0, 0.1);
    }

    [Fact]
    public void Timeline_NowLineAtEndDate_RendersAtSvgWidth()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-07-01",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var lineMatches = Regex.Matches(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""[^""]*""\s+x2=""([^""]+)""\s+y2=""[^""]*""\s+stroke=""#EA4335""");
        lineMatches.Count.Should().BeGreaterThan(0);
        var x1 = double.Parse(lineMatches[0].Groups[1].Value, CultureInfo.InvariantCulture);
        x1.Should().BeApproximately(1560, 0.1);
    }

    [Fact]
    public void Timeline_NowLineAtMidpoint_RendersNearCenter()
    {
        // Midpoint of Jan 1 to Jul 1 (181 days) is approx Apr 1 (90 days)
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var lineMatches = Regex.Matches(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""[^""]*""\s+x2=""([^""]+)""\s+y2=""[^""]*""\s+stroke=""#EA4335""");
        lineMatches.Count.Should().BeGreaterThan(0);
        var x1 = double.Parse(lineMatches[0].Groups[1].Value, CultureInfo.InvariantCulture);
        // 90 days / 181 days * 1560 ≈ 775.7
        x1.Should().BeInRange(700, 850, "NOW line should be roughly in the middle");
    }

    #endregion

    #region TrackToY Spacing Tests (via rendered SVG line positions)

    [Fact]
    public void Timeline_SingleTrack_YPositionIsCentered()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Track 1", Color = "#FF0000", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // With 1 track, y = 185 / (1+1) * 1 = 92.5
        var lineMatch = Regex.Match(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""([^""]+)""\s+x2=""1560""\s+y2=""([^""]+)""\s+stroke=""#FF0000""");
        lineMatch.Success.Should().BeTrue("Track line should exist");
        var y = double.Parse(lineMatch.Groups[1].Value, CultureInfo.InvariantCulture);
        y.Should().BeApproximately(92.5, 1.0, "Single track should be vertically centered");
    }

    [Fact]
    public void Timeline_ThreeTracks_EvenlySpaced()
    {
        var colors = new[] { "#FF0000", "#00FF00", "#0000FF" };
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = colors.Select((c, i) => new TimelineTrack
            {
                Name = $"M{i + 1}", Label = $"Track {i + 1}", Color = c, Milestones = new()
            }).ToList()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // spacing = 185 / 4 = 46.25, positions: 46.25, 92.5, 138.75
        var yPositions = new List<double>();
        foreach (var color in colors)
        {
            var match = Regex.Match(cut.Markup,
                $@"<line\s+x1=""0""\s+y1=""([^""]+)""\s+x2=""1560""\s+y2=""[^""]*""\s+stroke=""{Regex.Escape(color)}""");
            match.Success.Should().BeTrue($"Track line with color {color} should exist");
            yPositions.Add(double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
        }

        // Check even spacing
        var spacing1 = yPositions[1] - yPositions[0];
        var spacing2 = yPositions[2] - yPositions[1];
        spacing1.Should().BeApproximately(spacing2, 0.1, "Tracks should be evenly spaced");

        // All Y values should be within SVG bounds
        yPositions.Should().AllSatisfy(y =>
        {
            y.Should().BeGreaterThan(0);
            y.Should().BeLessThan(185);
        });
    }

    [Fact]
    public void Timeline_TenTracks_AllWithinSvgBounds()
    {
        var tl = CreateBasicTimeline(10);

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // With 10 tracks, spacing = 185/11 ≈ 16.8, all should fit
        var trackLines = Regex.Matches(cut.Markup,
            @"<line\s+x1=""0""\s+y1=""([^""]+)""\s+x2=""1560""\s+y2=""[^""]*""\s+stroke=""#");
        trackLines.Count.Should().Be(10);

        foreach (Match match in trackLines)
        {
            var y = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            y.Should().BeGreaterThan(0, "Y position should be positive");
            y.Should().BeLessThan(185, "Y position should be within SVG height");
        }
    }

    #endregion

    #region DiamondPoints Geometry Tests (via polygon points attribute)

    [Fact]
    public void Timeline_PocMilestone_RendersDiamondPolygon()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-04-01", Type = "poc", Label = "PoC" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");
        polygonMatch.Success.Should().BeTrue("PoC diamond polygon should exist");

        // Verify 4-point diamond shape: top, right, bottom, left
        var points = polygonMatch.Groups[1].Value.Split(' ');
        points.Should().HaveCount(4, "Diamond should have exactly 4 points");
    }

    [Fact]
    public void Timeline_DiamondPoints_FormSymmetricShape()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-04-01", Type = "poc", Label = "PoC" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");
        polygonMatch.Success.Should().BeTrue();

        // Parse points: "cx,cy-size cx+size,cy cx,cy+size cx-size,cy"
        var points = polygonMatch.Groups[1].Value.Split(' ')
            .Select(p => p.Split(',').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray())
            .ToArray();

        // Top and bottom points should have same X (center X)
        points[0][0].Should().BeApproximately(points[2][0], 0.1, "Top and bottom X should match (centered)");

        // Left and right points should have same Y (center Y)
        points[1][1].Should().BeApproximately(points[3][1], 0.1, "Left and right Y should match (centered)");

        // Diamond should be symmetric: top-to-center distance == bottom-to-center distance
        var centerY = points[1][1]; // right point Y is the center Y
        var topDist = centerY - points[0][1];
        var bottomDist = points[2][1] - centerY;
        topDist.Should().BeApproximately(bottomDist, 0.1, "Diamond should be vertically symmetric");

        var centerX = points[0][0]; // top point X is the center X
        var leftDist = centerX - points[3][0];
        var rightDist = points[1][0] - centerX;
        leftDist.Should().BeApproximately(rightDist, 0.1, "Diamond should be horizontally symmetric");
    }

    [Fact]
    public void Timeline_DiamondSize_IsApproximately11()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-04-01", Type = "poc", Label = "PoC" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup,
            @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");

        var points = polygonMatch.Groups[1].Value.Split(' ')
            .Select(p => p.Split(',').Select(v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray())
            .ToArray();

        // Default diamond size is 11; distance from center to top should be 11
        var centerY = points[1][1];
        var size = centerY - points[0][1];
        size.Should().BeApproximately(11, 0.5, "Diamond size should be approximately 11");
    }

    #endregion

    #region Milestone Type Rendering Tests

    [Fact]
    public void Timeline_PocMilestone_HasGoldFill()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        cut.Markup.Should().Contain("fill=\"#F4B400\"");
    }

    [Fact]
    public void Timeline_PocMilestone_HasDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        // PoC polygon should reference the shadow filter
        var pocPolygon = Regex.Match(cut.Markup,
            @"<polygon[^>]*fill=""#F4B400""[^>]*filter=""url\(#sh\)""");
        pocPolygon.Success.Should().BeTrue("PoC diamond should have drop shadow filter");
    }

    [Fact]
    public void Timeline_ProductionMilestone_HasGreenFill()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        cut.Markup.Should().Contain("fill=\"#34A853\"");
    }

    [Fact]
    public void Timeline_ProductionMilestone_HasDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        var prodPolygon = Regex.Match(cut.Markup,
            @"<polygon[^>]*fill=""#34A853""[^>]*filter=""url\(#sh\)""");
        prodPolygon.Success.Should().BeTrue("Production diamond should have drop shadow filter");
    }

    [Fact]
    public void Timeline_CheckpointMilestone_RendersCircle()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        // Checkpoint: circle with white fill and colored stroke
        cut.Markup.Should().Contain("fill=\"white\"");
        cut.Markup.Should().Contain("stroke=\"#4285F4\"");
        cut.Markup.Should().Contain("stroke-width=\"2.5\"");
    }

    [Fact]
    public void Timeline_CheckpointCircle_HasRadius7()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        var checkpointCircle = Regex.Match(cut.Markup,
            @"<circle[^>]*fill=""white""[^>]*r=""7""");
        checkpointCircle.Success.Should().BeTrue("Checkpoint circle should have radius 7");
    }

    [Fact]
    public void Timeline_SmallDotMilestone_RendersSmallCircle()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        // Small dot: circle r=4, fill #999
        var smallDot = Regex.Match(cut.Markup, @"<circle[^>]*r=""4""[^>]*fill=""#999""");
        smallDot.Success.Should().BeTrue("Small dot milestone should render as circle r=4 with #999 fill");
    }

    #endregion

    #region Tooltip Tests

    [Fact]
    public void Timeline_PocMilestone_HasTooltipWithLabelAndDate()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        cut.Markup.Should().Contain("<title>PoC Done - 2026-02-01</title>");
    }

    [Fact]
    public void Timeline_ProductionMilestone_HasTooltipWithLabelAndDate()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        cut.Markup.Should().Contain("<title>Go Live - 2026-03-01</title>");
    }

    [Fact]
    public void Timeline_CheckpointMilestone_HasTooltipWithLabelAndDate()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        cut.Markup.Should().Contain("<title>Review - 2026-04-01</title>");
    }

    [Fact]
    public void Timeline_SmallDotMilestone_HasTooltipWithLabelAndDate()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        cut.Markup.Should().Contain("<title>Minor - 2026-05-01</title>");
    }

    #endregion

    #region Milestone Date Label Tests

    [Fact]
    public void Timeline_MilestoneDateLabels_HaveCorrectTextAttributes()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        // Date labels: 10px font, #666 fill, text-anchor middle
        var dateLabel = Regex.Match(cut.Markup,
            @"<text[^>]*font-size=""10""[^>]*text-anchor=""middle""[^>]*fill=""#666""");
        // Try alternate attribute ordering
        if (!dateLabel.Success)
        {
            dateLabel = Regex.Match(cut.Markup,
                @"<text[^>]*fill=""#666""[^>]*font-size=""10""[^>]*text-anchor=""middle""");
        }
        dateLabel.Success.Should().BeTrue("Milestone date labels should have correct attributes");
    }

    [Fact]
    public void Timeline_MilestoneDateLabels_DisplayLabelText()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("Feb 15");
        cut.Markup.Should().Contain("May 1");
        cut.Markup.Should().Contain("Mar 15");
    }

    #endregion

    #region NOW Line Tests

    [Fact]
    public void Timeline_NowLine_HasRedStroke()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("stroke=\"#EA4335\"");
    }

    [Fact]
    public void Timeline_NowLine_HasDashedStrokeArray()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
    }

    [Fact]
    public void Timeline_NowLine_HasStrokeWidth2()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("stroke-width=\"2\"");
    }

    [Fact]
    public void Timeline_NowLabel_DisplaysBoldRedText()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        cut.Markup.Should().Contain("NOW");
        var nowLabel = Regex.Match(cut.Markup,
            @"<text[^>]*fill=""#EA4335""[^>]*font-weight=""700""[^>]*>NOW</text>");
        if (!nowLabel.Success)
        {
            nowLabel = Regex.Match(cut.Markup,
                @"<text[^>]*font-weight=""700""[^>]*fill=""#EA4335""[^>]*>NOW</text>");
        }
        nowLabel.Success.Should().BeTrue("NOW label should be bold red");
    }

    [Fact]
    public void Timeline_NowLabel_HasFontSize10()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var nowText = Regex.Match(cut.Markup, @"<text[^>]*font-size=""10""[^>]*>NOW</text>");
        nowText.Success.Should().BeTrue("NOW label should have font-size 10");
    }

    [Fact]
    public void Timeline_NowLabel_HasTextAnchorMiddle()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var nowText = Regex.Match(cut.Markup, @"<text[^>]*text-anchor=""middle""[^>]*>NOW</text>");
        nowText.Success.Should().BeTrue("NOW label should have text-anchor middle");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Timeline_NullTimelineData_RendersWithoutError()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, (TimelineData?)null));

        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_NullTimelineData_StillRendersNowLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, (TimelineData?)null));

        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("#EA4335");
    }

    [Fact]
    public void Timeline_NullTracks_RendersNowLineOnly()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = null!
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        cut.Markup.Should().Contain("NOW");
        cut.FindAll("polygon").Count.Should().Be(0, "No milestones should render with null tracks");
    }

    [Fact]
    public void Timeline_EmptyTracks_RendersGracefully()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateEmptyTimeline()));

        cut.Find(".tl-area").Should().NotBeNull();
        cut.Find("svg").Should().NotBeNull();
        cut.FindAll(".tl-track-label").Count.Should().Be(0);
    }

    [Fact]
    public void Timeline_EmptyTracks_StillRendersNowLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateEmptyTimeline()));

        cut.Markup.Should().Contain("stroke=\"#EA4335\"");
        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");
        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public void Timeline_NullMilestones_TrackStillRendersLine()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Track", Color = "#FF0000", Milestones = null! }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // Track line should still render
        cut.Markup.Should().Contain("stroke=\"#FF0000\"");
        cut.Markup.Should().Contain("stroke-width=\"3\"");
    }

    [Fact]
    public void Timeline_InvalidMilestoneDate_SkipsMilestone()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "not-a-date", Type = "poc", Label = "Bad Date" },
                        new() { Date = "2026-04-01", Type = "poc", Label = "Good Date" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // "Bad Date" milestone should be skipped; "Good Date" should render
        cut.Markup.Should().Contain("Good Date");
        cut.Markup.Should().NotContain("Bad Date");
    }

    [Fact]
    public void Timeline_MilestoneBeforeStartDate_ClampsToZero()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-03-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-01-01", Type = "poc", Label = "Early" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // The milestone should be clamped to 0 (not negative)
        var polygonMatch = Regex.Match(cut.Markup, @"<polygon\s+points=""([^""]+)""");
        polygonMatch.Success.Should().BeTrue();

        var firstPoint = polygonMatch.Groups[1].Value.Split(' ')[0].Split(',');
        var cx = double.Parse(firstPoint[0], CultureInfo.InvariantCulture);
        cx.Should().BeGreaterOrEqualTo(0, "Clamped milestone X should not be negative");
    }

    [Fact]
    public void Timeline_MilestoneAfterEndDate_ClampsToSvgWidth()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-03-01", NowDate = "2026-02-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-07-01", Type = "poc", Label = "Late" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup, @"<polygon\s+points=""([^""]+)""");
        polygonMatch.Success.Should().BeTrue();

        // Parse the diamond center X from the top point (first point, cx is the X)
        var firstPoint = polygonMatch.Groups[1].Value.Split(' ')[0].Split(',');
        var cx = double.Parse(firstPoint[0], CultureInfo.InvariantCulture);
        cx.Should().BeLessOrEqualTo(1560, "Clamped milestone X should not exceed SVG width");
    }

    [Fact]
    public void Timeline_SameStartAndEndDate_DateToXReturnsZero()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-04-01", EndDate = "2026-04-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        // When start == end, totalDays is 0, DateToX should return 0
        var lineMatches = Regex.Matches(cut.Markup,
            @"<line\s+x1=""([^""]+)""\s+y1=""[^""]*""\s+x2=""([^""]+)""\s+y2=""[^""]*""\s+stroke=""#EA4335""");
        lineMatches.Count.Should().BeGreaterThan(0);
        var x1 = double.Parse(lineMatches[0].Groups[1].Value, CultureInfo.InvariantCulture);
        x1.Should().Be(0, "When start equals end date, DateToX should return 0");
    }

    [Fact]
    public void Timeline_InvalidStartDate_DefaultsToToday()
    {
        var tl = new TimelineData
        {
            StartDate = "invalid", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>()
        };

        // Should not throw
        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_InvalidEndDate_DefaultsToSixMonthsFromToday()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "invalid", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public void Timeline_InvalidNowDate_DefaultsToToday()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "invalid",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        cut.Markup.Should().Contain("NOW");
    }

    #endregion

    #region Single Milestone Positioning Tests

    [Fact]
    public void Timeline_SingleMilestoneAtKnownDate_RendersAtCorrectXPosition()
    {
        // Place a milestone exactly at the midpoint
        var tl = new TimelineData
        {
            StartDate = "2026-01-01", EndDate = "2026-07-01", NowDate = "2026-04-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Track", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-01-01", Type = "poc", Label = "Start" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));

        var polygonMatch = Regex.Match(cut.Markup, @"<polygon\s+points=""([^""]+)""\s+fill=""#F4B400""");
        polygonMatch.Success.Should().BeTrue();

        // At start date, cx should be 0 (clamped to 0)
        var topPoint = polygonMatch.Groups[1].Value.Split(' ')[0].Split(',');
        var cx = double.Parse(topPoint[0], CultureInfo.InvariantCulture);
        cx.Should().BeApproximately(0, 1.0, "Milestone at start date should be at X=0");
    }

    #endregion

    #region SVG Height Consistency Tests

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Timeline_SvgHeight_AlwaysIs185_RegardlessOfTrackCount(int trackCount)
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline(trackCount)));

        var svg = cut.Find("svg");
        svg.GetAttribute("height").Should().Be("185");
    }

    #endregion

    #region Font Family Tests

    [Fact]
    public void Timeline_SvgHasCorrectFontFamily()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        var style = svg.GetAttribute("style") ?? "";
        style.Should().Contain("Segoe UI");
    }

    #endregion

    #region Multiple Milestone Types on Same Track

    [Fact]
    public void Timeline_TrackWithAllMilestoneTypes_RendersAll()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimelineWithMilestoneTypes()));

        var markup = cut.Markup;

        // PoC diamond (gold)
        markup.Should().Contain("fill=\"#F4B400\"");
        // Production diamond (green)
        markup.Should().Contain("fill=\"#34A853\"");
        // Checkpoint circle (white fill, colored stroke)
        markup.Should().Contain("fill=\"white\"");
        // Small dot (gray)
        markup.Should().Contain("fill=\"#999\"");

        // Count polygons (poc + production = 2)
        var polygonCount = Regex.Matches(markup, @"<polygon\s").Count;
        polygonCount.Should().Be(2, "Should have 2 diamond polygons (poc + production)");
    }

    #endregion

    #region Rendering Stability Tests

    [Fact]
    public void Timeline_ReRenderWithSameData_ProducesSameMarkup()
    {
        var tl = CreateBasicTimeline(3);

        var cut1 = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        var markup1 = cut1.Markup;

        var cut2 = RenderComponent<Timeline>(p => p.Add(x => x.TimelineData, tl));
        var markup2 = cut2.Markup;

        markup1.Should().Be(markup2, "Same data should produce identical markup");
    }

    #endregion
}