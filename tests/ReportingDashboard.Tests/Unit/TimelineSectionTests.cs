using System.Text.Json;
using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class TimelineSectionTests : IDisposable
{
    private readonly Bunit.TestContext _ctx;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public TimelineSectionTests()
    {
        _ctx = new Bunit.TestContext();
    }

    public void Dispose()
    {
        _ctx.Dispose();
    }

    private static DashboardData DeserializeData(string json)
    {
        return JsonSerializer.Deserialize<DashboardData>(json, JsonOptions)!;
    }

    private static DashboardData CreateBasicData()
    {
        var json = """
        {
            "title": "Test Dashboard",
            "subtitle": "Test Subtitle",
            "backlogUrl": "https://example.com",
            "currentDate": "2026-02-15",
            "months": ["Jan", "Feb", "Mar"],
            "tracks": [
                {
                    "id": "M1",
                    "label": "Chatbot & MS Role",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-01-15", "type": "Checkpoint", "label": "Alpha" },
                        { "date": "2026-02-20", "type": "PoC", "label": "Beta PoC" },
                        { "date": "2026-03-10", "type": "Production", "label": "GA" }
                    ]
                },
                {
                    "id": "M2",
                    "label": "PDS & Data Inventory",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-02-01", "type": "Checkpoint", "label": "Review" }
                    ]
                }
            ],
            "statusRows": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        return DeserializeData(json);
    }

    [Fact]
    public void Renders_TrackLabels_InSidebar_WithCorrectStyling()
    {
        var data = CreateBasicData();

        var cut = _ctx.RenderComponent<TimelineSection>(p => p.Add(x => x.Data, data));

        var sidebar = cut.Find(".tl-sidebar");
        var trackLabels = sidebar.QuerySelectorAll(".tl-track-label");
        trackLabels.Should().HaveCount(2);

        // First track label
        var firstLabel = trackLabels[0];
        var spans = firstLabel.QuerySelectorAll("span");
        spans[0].TextContent.Should().Be("M1");
        spans[0].GetAttribute("style").Should().Contain("color:#0078D4");
        spans[0].GetAttribute("style").Should().Contain("font-weight:600");
        spans[1].TextContent.Should().Be("Chatbot & MS Role");
        spans[1].GetAttribute("style").Should().Contain("color:#444");

        // Second track label
        var secondLabel = trackLabels[1];
        var spans2 = secondLabel.QuerySelectorAll("span");
        spans2[0].TextContent.Should().Be("M2");
        spans2[0].GetAttribute("style").Should().Contain("color:#00897B");
        spans2[1].TextContent.Should().Be("PDS & Data Inventory");
    }

    [Fact]
    public void Renders_CorrectSvgShapes_ForDifferentMilestoneTypes()
    {
        var data = CreateBasicData();

        var cut = _ctx.RenderComponent<TimelineSection>(p => p.Add(x => x.Data, data));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
        svg.GetAttribute("height").Should().Be("185");

        // Checkpoint milestone renders as circle with white fill and track-color stroke
        var circles = svg.QuerySelectorAll("circle");
        circles.Length.Should().BeGreaterOrEqualTo(1);
        var checkpointCircle = circles.First(c => c.GetAttribute("stroke") == "#0078D4");
        checkpointCircle.GetAttribute("r").Should().Be("6");
        checkpointCircle.GetAttribute("fill").Should().Be("white");
        checkpointCircle.GetAttribute("stroke-width").Should().Be("2.5");

        // PoC milestone renders as polygon with gold fill
        var polygons = svg.QuerySelectorAll("polygon");
        var pocPolygon = polygons.First(p => p.GetAttribute("fill") == "#F4B400");
        pocPolygon.Should().NotBeNull();
        pocPolygon.GetAttribute("filter").Should().Be("url(#sh)");

        // Production milestone renders as polygon with green fill
        var prodPolygon = polygons.First(p => p.GetAttribute("fill") == "#34A853");
        prodPolygon.Should().NotBeNull();
        prodPolygon.GetAttribute("filter").Should().Be("url(#sh)");
    }

    [Fact]
    public void Renders_NowIndicator_WhenCurrentDateIsValid()
    {
        var data = CreateBasicData();

        var cut = _ctx.RenderComponent<TimelineSection>(p => p.Add(x => x.Data, data));

        // NOW dashed line: stroke="#EA4335", stroke-dasharray="5,3"
        var allLines = cut.FindAll("svg line");
        var nowLine = allLines.FirstOrDefault(l =>
            l.GetAttribute("stroke") == "#EA4335" &&
            l.GetAttribute("stroke-dasharray") == "5,3");
        nowLine.Should().NotBeNull("a dashed red NOW line should be rendered");
        nowLine!.GetAttribute("stroke-width").Should().Be("2");
        nowLine.GetAttribute("y1").Should().Be("0");
        nowLine.GetAttribute("y2").Should().Be("185");

        // NOW text label
        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public void DoesNotCrash_WithEmptyTracks()
    {
        var json = """
        {
            "currentDate": "2026-02-15",
            "months": ["Jan", "Feb", "Mar"],
            "tracks": [],
            "statusRows": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var data = DeserializeData(json);

        var cut = _ctx.RenderComponent<TimelineSection>(p => p.Add(x => x.Data, data));

        var sidebar = cut.Find(".tl-sidebar");
        sidebar.QuerySelectorAll(".tl-track-label").Should().BeEmpty();

        // SVG should still render month gridlines
        var svg = cut.Find("svg");
        svg.Should().NotBeNull();

        // No track lines (stroke-width="3") since no tracks
        var svgLines = svg.QuerySelectorAll("line[stroke-width='3']");
        svgLines.Should().BeEmpty();
    }

    [Fact]
    public void DoesNotRenderNow_WhenCurrentDateIsMissing()
    {
        var json = """
        {
            "currentDate": "",
            "months": ["Jan", "Feb", "Mar"],
            "tracks": [
                {
                    "id": "M1",
                    "label": "Track 1",
                    "color": "#0078D4",
                    "milestones": []
                }
            ],
            "statusRows": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var data = DeserializeData(json);

        var cut = _ctx.RenderComponent<TimelineSection>(p => p.Add(x => x.Data, data));

        // No dashed red line
        var allLines = cut.FindAll("svg line");
        var nowLine = allLines.FirstOrDefault(l =>
            l.GetAttribute("stroke") == "#EA4335" &&
            l.GetAttribute("stroke-dasharray") == "5,3");
        nowLine.Should().BeNull("NOW indicator should not render when CurrentDate is empty");

        // "NOW" text should not appear in SVG
        var svgMarkup = cut.Find("svg").InnerHtml;
        svgMarkup.Should().NotContain(">NOW<");
    }
}