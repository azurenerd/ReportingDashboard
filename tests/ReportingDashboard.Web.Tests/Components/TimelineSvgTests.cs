using Bunit;
using ReportingDashboard.Web.Components.Pages.Partials;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Components;

public class TimelineSvgTests : TestContext
{
    private static TimelineViewModel BuildModel(
        NowMarker? now = null,
        IReadOnlyList<LaneGeometry>? lanes = null,
        IReadOnlyList<MonthGridline>? gridlines = null)
    {
        return new TimelineViewModel(
            gridlines ?? new[] { new MonthGridline(0, "Jan"), new MonthGridline(260, "Feb") },
            lanes ?? Array.Empty<LaneGeometry>(),
            now ?? new NowMarker(0, false));
    }

    [Fact]
    public void Renders_Empty_Model_Without_Throwing()
    {
        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, TimelineViewModel.Empty));

        cut.Find("svg").Should().NotBeNull();
        cut.Find("filter#sh").Should().NotBeNull();
        cut.Find("feDropShadow").Should().NotBeNull();
        cut.FindAll("line").Should().BeEmpty();
    }

    [Fact]
    public void Renders_Month_Gridlines_And_Labels()
    {
        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, BuildModel()));

        var texts = cut.FindAll("text");
        texts.Select(t => t.TextContent).Should().Contain(new[] { "Jan", "Feb" });

        var gridLines = cut.FindAll("line").Where(l =>
            l.GetAttribute("stroke") == "#bbb").ToList();
        gridLines.Should().HaveCount(2);
        gridLines[0].GetAttribute("stroke-opacity").Should().Be("0.4");
    }

    [Fact]
    public void Renders_Lane_Tracks_And_Labels()
    {
        var lane = new LaneGeometry(
            "M1", "Chatbot", "#0078D4", 42,
            Array.Empty<MilestoneGeometry>());

        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, BuildModel(lanes: new[] { lane })));

        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("Chatbot");

        var track = cut.FindAll("line").First(l => l.GetAttribute("stroke") == "#0078D4");
        track.GetAttribute("stroke-width").Should().Be("3");
        track.GetAttribute("y1").Should().Be("42");
    }

    [Fact]
    public void Renders_Poc_As_Amber_Diamond_With_Shadow()
    {
        var lane = new LaneGeometry("M1", "L", "#0078D4", 42, new[]
        {
            new MilestoneGeometry(745, 42, MilestoneType.Poc, "Mar 26 PoC", CaptionPosition.Below)
        });
        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, BuildModel(lanes: new[] { lane })));

        var poly = cut.Find("polygon");
        poly.GetAttribute("fill").Should().Be("#F4B400");
        poly.GetAttribute("filter").Should().Be("url(#sh)");
        poly.GetAttribute("points").Should().Contain("745");
    }

    [Fact]
    public void Renders_Prod_As_Green_Diamond()
    {
        var lane = new LaneGeometry("M1", "L", "#0078D4", 42, new[]
        {
            new MilestoneGeometry(1040, 42, MilestoneType.Prod, "Apr Prod", CaptionPosition.Above)
        });
        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, BuildModel(lanes: new[] { lane })));

        var poly = cut.Find("polygon");
        poly.GetAttribute("fill").Should().Be("#34A853");
        poly.GetAttribute("filter").Should().Be("url(#sh)");
    }

    [Fact]
    public void Renders_Checkpoint_As_Stroked_White_Circle()
    {
        var lane = new LaneGeometry("M1", "L", "#0078D4", 42, new[]
        {
            new MilestoneGeometry(104, 42, MilestoneType.Checkpoint, "Jan 12", CaptionPosition.Above)
        });
        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, BuildModel(lanes: new[] { lane })));

        var circle = cut.Find("circle");
        circle.GetAttribute("fill").Should().Be("white");
        circle.GetAttribute("stroke").Should().Be("#0078D4");
        circle.GetAttribute("cx").Should().Be("104");
    }

    [Fact]
    public void Renders_Now_Line_Only_When_InRange()
    {
        var inRange = RenderComponent<TimelineSvg>(p =>
            p.Add(x => x.Model, BuildModel(now: new NowMarker(823, true))));
        var nowLine = inRange.FindAll("line").First(l => l.GetAttribute("stroke") == "#EA4335");
        nowLine.GetAttribute("stroke-dasharray").Should().Be("5,3");
        inRange.Markup.Should().Contain(">NOW<");

        var outOfRange = RenderComponent<TimelineSvg>(p =>
            p.Add(x => x.Model, BuildModel(now: new NowMarker(0, false))));
        outOfRange.FindAll("line").Any(l => l.GetAttribute("stroke") == "#EA4335").Should().BeFalse();
        outOfRange.Markup.Should().NotContain(">NOW<");
    }

    [Fact]
    public void Caption_Position_Above_And_Below_Offsets_From_Lane_Y()
    {
        var lane = new LaneGeometry("M1", "L", "#0078D4", 100, new[]
        {
            new MilestoneGeometry(200, 100, MilestoneType.Poc, "Above", CaptionPosition.Above),
            new MilestoneGeometry(400, 100, MilestoneType.Poc, "Below", CaptionPosition.Below)
        });
        var cut = RenderComponent<TimelineSvg>(p => p.Add(x => x.Model, BuildModel(lanes: new[] { lane })));

        var above = cut.FindAll("text").First(t => t.TextContent == "Above");
        var below = cut.FindAll("text").First(t => t.TextContent == "Below");

        double.Parse(above.GetAttribute("y")!, System.Globalization.CultureInfo.InvariantCulture)
            .Should().BeLessThan(100);
        double.Parse(below.GetAttribute("y")!, System.Globalization.CultureInfo.InvariantCulture)
            .Should().BeGreaterThan(100);
    }
}
