using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the CSS classes used in the legend section of Components/Header.razor.
/// Verifies the specific class names: legend, legend-item, legend-diamond, legend-poc,
/// legend-prod, legend-circle, legend-now-line which are used for styling.
/// Existing HeaderInlineTests (truncated) may not fully cover these class selectors.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderLegendCssClassTests : TestContext
{
    private static DashboardData CreateData() => new()
    {
        Title = "Test",
        Subtitle = "Sub",
        BacklogLink = "https://link",
        CurrentMonth = "April",
        Months = new List<string> { "April" },
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        },
        Heatmap = new HeatmapData()
    };

    #region Legend Container

    [Fact]
    public void Header_HasLegendContainer()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var legend = cut.Find(".legend");
        Assert.NotNull(legend);
    }

    [Fact]
    public void Header_LegendContainsExactlyFourItems()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var items = cut.FindAll(".legend-item");
        Assert.Equal(4, items.Count);
    }

    #endregion

    #region PoC Milestone Legend Item

    [Fact]
    public void Header_Legend_HasPocDiamondWithCorrectClasses()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var pocDiamond = cut.Find(".legend-diamond.legend-poc");
        Assert.NotNull(pocDiamond);
    }

    [Fact]
    public void Header_Legend_PocItemContainsLabelText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("PoC Milestone", cut.Markup);
    }

    #endregion

    #region Production Release Legend Item

    [Fact]
    public void Header_Legend_HasProdDiamondWithCorrectClasses()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var prodDiamond = cut.Find(".legend-diamond.legend-prod");
        Assert.NotNull(prodDiamond);
    }

    [Fact]
    public void Header_Legend_ProdItemContainsLabelText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("Production Release", cut.Markup);
    }

    #endregion

    #region Checkpoint Legend Item

    [Fact]
    public void Header_Legend_HasCircleElement()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var circle = cut.Find(".legend-circle");
        Assert.NotNull(circle);
    }

    [Fact]
    public void Header_Legend_CheckpointItemContainsLabelText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("Checkpoint", cut.Markup);
    }

    #endregion

    #region Now Line Legend Item

    [Fact]
    public void Header_Legend_HasNowLineElement()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var nowLine = cut.Find(".legend-now-line");
        Assert.NotNull(nowLine);
    }

    [Fact]
    public void Header_Legend_NowItemContainsDynamicMonth()
    {
        var data = CreateData();
        data.CurrentMonth = "July";
        data.Timeline!.NowDate = "2026-07-15";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (July 2026)", cut.Markup);
    }

    #endregion

    #region H1 CSS Class

    [Fact]
    public void Header_H1_HasHdrTitleClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var h1 = cut.Find("h1.hdr-title");
        Assert.NotNull(h1);
    }

    #endregion

    #region Legend Symbol Count Validation

    [Fact]
    public void Header_Legend_HasExactlyTwoDiamonds()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var diamonds = cut.FindAll(".legend-diamond");
        Assert.Equal(2, diamonds.Count);
    }

    [Fact]
    public void Header_Legend_HasExactlyOneCircle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var circles = cut.FindAll(".legend-circle");
        Assert.Single(circles);
    }

    [Fact]
    public void Header_Legend_HasExactlyOneNowLine()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var nowLines = cut.FindAll(".legend-now-line");
        Assert.Single(nowLines);
    }

    #endregion
}