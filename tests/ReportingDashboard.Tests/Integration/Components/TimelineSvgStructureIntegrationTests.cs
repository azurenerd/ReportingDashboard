using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using System.Globalization;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests focused on verifying the SVG structural output of the
/// Timeline component, validating coordinate calculations, element hierarchy,
/// and visual fidelity markers against the design specification.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineSvgStructureIntegrationTests : TestContext
{
    private static TimelineData CreateStandardTimeline()
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
                    Id = "M1", Name = "Track 1", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-01-01", Label = "Start", Type = "poc" },
                        new() { Date = "2026-06-30", Label = "End", Type = "production" },
                        new() { Date = "2026-04-01", Label = "Mid", Type = "checkpoint" }
                    }
                }
            }
        };
    }

    [Fact]
    public void Svg_DefsSection_ComesBeforeContent()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;
        var defsIndex = markup.IndexOf("<defs>");
        var lineIndex = markup.IndexOf("stroke-width=\"3\"");

        defsIndex.Should().BeGreaterThanOrEqualTo(0);
        lineIndex.Should().BeGreaterThanOrEqualTo(0);
        defsIndex.Should().BeLessThan(lineIndex, "defs should come before track content");
    }

    [Fact]
    public void Svg_PocDiamondAtStartDate_XPositionNearZero()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygons = cut.FindAll("polygon");
        var pocPolygon = polygons.First(p => p.GetAttribute("fill") == "#F4B400");
        var points = pocPolygon.GetAttribute("points")!;

        // Parse first vertex (top of diamond): "cx,cy-r"
        var firstVertex = points.Split(' ')[0].Split(',');
        var cx = double.Parse(firstVertex[0], CultureInfo.InvariantCulture);

        // Start date should map to x ≈ 0
        cx.Should().BeApproximately(0.0, 1.0);
    }

    [Fact]
    public void Svg_ProductionDiamondAtEndDate_XPositionNear1560()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygons = cut.FindAll("polygon");
        var prodPolygon = polygons.First(p => p.GetAttribute("fill") == "#34A853");
        var points = prodPolygon.GetAttribute("points")!;

        var firstVertex = points.Split(' ')[0].Split(',');
        var cx = double.Parse(firstVertex[0], CultureInfo.InvariantCulture);

        // End date should map to x ≈ 1560
        cx.Should().BeApproximately(1560.0, 1.0);
    }

    [Fact]
    public void Svg_CheckpointAtApril1_XPositionNearMiddle()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var circle = cut.Find("circle");
        var cxStr = circle.GetAttribute("cx");
        var cx = double.Parse(cxStr!, CultureInfo.InvariantCulture);

        // April 1 is ~day 90 of 180 total days ≈ 780px
        cx.Should().BeInRange(720, 840);
    }

    [Fact]
    public void Svg_DiamondVertices_FormValidRhombus()
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
                        new() { Date = "2026-03-15", Label = "PoC", Type = "poc" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var polygon = cut.Find("polygon");
        var points = polygon.GetAttribute("points")!;
        var vertices = points.Split(' ')
            .Select(p => p.Split(','))
            .Select(c => (X: double.Parse(c[0], CultureInfo.InvariantCulture),
                          Y: double.Parse(c[1], CultureInfo.InvariantCulture)))
            .ToArray();

        vertices.Should().HaveCount(4);

        // Rhombus: top and bottom share same X (center), left and right share same Y (center)
        var topX = vertices[0].X;
        var bottomX = vertices[2].X;
        topX.Should().BeApproximately(bottomX, 0.01, "top and bottom vertices should have same X");

        var leftY = vertices[3].Y;
        var rightY = vertices[1].Y;
        leftY.Should().BeApproximately(rightY, 0.01, "left and right vertices should have same Y");

        // Diamond radius should be 9 (as defined in component)
        var radius = vertices[1].X - vertices[0].X;
        radius.Should().BeApproximately(9.0, 0.1);
    }

    [Fact]
    public void Svg_TrackYPositions_AreEvenlySpaced()
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
                    Id = "M1", Name = "T1", Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "M1", Type = "checkpoint" }
                    }
                },
                new()
                {
                    Id = "M2", Name = "T2", Color = "#00897B",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "M2", Type = "checkpoint" }
                    }
                },
                new()
                {
                    Id = "M3", Name = "T3", Color = "#546E7A",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-15", Label = "M3", Type = "checkpoint" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var circles = cut.FindAll("circle");
        circles.Should().HaveCount(3);

        var yPositions = circles
            .Select(c => double.Parse(c.GetAttribute("cy")!, CultureInfo.InvariantCulture))
            .ToList();

        // firstTrackY=42, spacing=56
        // Track 1: y=42, Track 2: y=98, Track 3: y=154
        yPositions[0].Should().BeApproximately(42, 1);
        yPositions[1].Should().BeApproximately(98, 1);
        yPositions[2].Should().BeApproximately(154, 1);

        // Spacing between tracks should be uniform (56px)
        var spacing1 = yPositions[1] - yPositions[0];
        var spacing2 = yPositions[2] - yPositions[1];
        spacing1.Should().BeApproximately(spacing2, 0.1);
        spacing1.Should().BeApproximately(56, 0.1);
    }

    [Fact]
    public void Svg_MonthGridLines_XPositionsAreMonotonicallyIncreasing()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Find all line elements with grid line characteristics
        var allLines = cut.FindAll("line");
        var gridLines = allLines
            .Where(l => l.GetAttribute("stroke") == "#bbb")
            .ToList();

        gridLines.Should().HaveCountGreaterOrEqualTo(6, "Jan-Jun should produce at least 6 grid lines");

        var xPositions = gridLines
            .Select(l => double.Parse(l.GetAttribute("x1")!, CultureInfo.InvariantCulture))
            .ToList();

        for (int i = 1; i < xPositions.Count; i++)
        {
            xPositions[i].Should().BeGreaterThan(xPositions[i - 1],
                "month grid line X positions should be monotonically increasing");
        }
    }

    [Fact]
    public void Svg_NowLineXPosition_IsBetweenAprilAndMayGridLines()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        // Find NOW line (EA4335 stroke)
        var allLines = cut.FindAll("line");
        var nowLine = allLines
            .FirstOrDefault(l => l.GetAttribute("stroke") == "#EA4335");

        nowLine.Should().NotBeNull("NOW line should exist");
        var nowX = double.Parse(nowLine!.GetAttribute("x1")!, CultureInfo.InvariantCulture);

        // Find grid lines
        var gridLines = allLines
            .Where(l => l.GetAttribute("stroke") == "#bbb")
            .Select(l => double.Parse(l.GetAttribute("x1")!, CultureInfo.InvariantCulture))
            .OrderBy(x => x)
            .ToList();

        // April is the 4th month (index 3), May is 5th (index 4)
        // Apr 10 should be between Apr 1 and May 1 grid lines
        gridLines.Count.Should().BeGreaterOrEqualTo(5);
        nowX.Should().BeGreaterThan(gridLines[3], "NOW (Apr 10) should be after April grid line");
        nowX.Should().BeLessThan(gridLines[4], "NOW (Apr 10) should be before May grid line");
    }

    [Fact]
    public void Svg_NowLine_SpansFullHeight()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var allLines = cut.FindAll("line");
        var nowLine = allLines.First(l => l.GetAttribute("stroke") == "#EA4335");

        nowLine.GetAttribute("y1").Should().Be("0");

        var svg = cut.Find("svg");
        var svgHeight = svg.GetAttribute("height");
        nowLine.GetAttribute("y2").Should().Be(svgHeight);
    }

    [Fact]
    public void Svg_GridLines_SpanFullHeight()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var allLines = cut.FindAll("line");
        var gridLines = allLines.Where(l => l.GetAttribute("stroke") == "#bbb").ToList();

        var svg = cut.Find("svg");
        var svgHeight = svg.GetAttribute("height");

        gridLines.Should().AllSatisfy(l =>
        {
            l.GetAttribute("y1").Should().Be("0");
            l.GetAttribute("y2").Should().Be(svgHeight);
        });
    }

    [Fact]
    public void Svg_XmlnsAttribute_IsPresent()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var svg = cut.Find("svg");
        svg.GetAttribute("xmlns").Should().Be("http://www.w3.org/2000/svg");
    }

    [Fact]
    public void Svg_FilterElement_HasCorrectDropShadowProperties()
    {
        var timeline = CreateStandardTimeline();

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineModel, timeline));

        var markup = cut.Markup;

        // Filter dimensions
        markup.Should().Contain("x=\"-20%\"");
        markup.Should().Contain("y=\"-20%\"");
        markup.Should().Contain("width=\"140%\"");
        markup.Should().Contain("height=\"140%\"");

        // DropShadow properties
        markup.Should().Contain("dx=\"0\"");
        markup.Should().Contain("dy=\"1\"");
        markup.Should().Contain("stdDeviation=\"1.5\"");
        markup.Should().Contain("flood-opacity=\"0.3\"");
    }
}