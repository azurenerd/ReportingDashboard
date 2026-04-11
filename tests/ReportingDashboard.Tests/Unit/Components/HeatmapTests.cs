using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeatmapTests : TestContext
{
    private static HeatmapData CreateTestHeatmap() => new()
    {
        Shipped = new Dictionary<string, List<string>>
        {
            ["Jan"] = new() { "Feature A" },
            ["Feb"] = new() { "Feature B" }
        },
        InProgress = new Dictionary<string, List<string>>
        {
            ["Mar"] = new() { "Task X" }
        },
        Carryover = new Dictionary<string, List<string>>(),
        Blockers = new Dictionary<string, List<string>>
        {
            ["Mar"] = new() { "Blocker 1" }
        }
    };

    [Fact]
    public void Heatmap_ShouldRenderTitle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, "Mar"));

        cut.Find(".hm-title").TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
    }

    [Fact]
    public void Heatmap_ShouldRenderColumnHeaders()
    {
        var months = new List<string> { "Jan", "Feb", "Mar" };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Mar"));

        var headers = cut.FindAll(".hm-col-hdr");
        headers.Should().HaveCount(3);
    }

    [Fact]
    public void Heatmap_CurrentMonthHeader_ShouldHaveCurrentClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, "Mar"));

        cut.Find(".cur-month-hdr").TextContent.Should().Contain("Mar");
        cut.Find(".cur-month-hdr").TextContent.Should().Contain("Now");
    }

    [Fact]
    public void Heatmap_NonCurrentMonthHeaders_ShouldNotHaveCurrentClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, "Mar"));

        var nonCurrentHeaders = cut.FindAll(".hm-col-hdr:not(.cur-month-hdr)");
        nonCurrentHeaders.Should().HaveCount(2);
    }

    [Fact]
    public void Heatmap_ShouldRenderStatusCorner()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        cut.Find(".hm-corner").TextContent.Should().Be("STATUS");
    }

    [Fact]
    public void Heatmap_ShouldRenderFourCategoryRows()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        cut.FindAll(".hm-row-hdr").Should().HaveCount(4);
    }

    [Fact]
    public void Heatmap_WithNullHeatmapModel_ShouldRenderWithDefaults()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, (HeatmapData?)null)
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        // Should render without throwing
        cut.Find(".hm-grid").Should().NotBeNull();
    }

    [Fact]
    public void Heatmap_GridShouldHaveCorrectColumnTemplate()
    {
        var months = new List<string> { "Jan", "Feb", "Mar", "Apr" };

        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, months)
            .Add(x => x.CurrentMonth, "Apr"));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("repeat(4, 1fr)");
    }

    [Fact]
    public void Heatmap_WithSingleMonth_ShouldRenderGrid()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Apr" })
            .Add(x => x.CurrentMonth, "Apr"));

        var style = cut.Find(".hm-grid").GetAttribute("style");
        style.Should().Contain("repeat(1, 1fr)");
    }

    [Fact]
    public void Heatmap_TitleShouldContainAllCategories()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Heatmap>(p => p
            .Add(x => x.HeatmapModel, CreateTestHeatmap())
            .Add(x => x.Months, new List<string> { "Jan" })
            .Add(x => x.CurrentMonth, "Jan"));

        var title = cut.Find(".hm-title").TextContent;
        title.Should().Contain("Shipped");
        title.Should().Contain("In Progress");
        title.Should().Contain("Carryover");
        title.Should().Contain("Blockers");
    }
}