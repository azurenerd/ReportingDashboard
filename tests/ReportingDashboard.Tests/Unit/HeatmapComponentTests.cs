using Bunit;
using FluentAssertions;
using ReportingDashboard.Components.Shared;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class HeatmapComponentTests : TestContext
{
    private static HeatmapData CreateTestHeatmapData()
    {
        return new HeatmapData
        {
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            CurrentMonthIndex = 3,
            Rows = new List<HeatmapRow>
            {
                new HeatmapRow
                {
                    Category = "shipped",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["Jan"] = new List<string> { "Feature X" },
                        ["Apr"] = new List<string> { "Feature Y", "Feature Z" }
                    }
                },
                new HeatmapRow
                {
                    Category = "in-progress",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["Apr"] = new List<string> { "Work Item A" }
                    }
                },
                new HeatmapRow
                {
                    Category = "carryover",
                    Items = new Dictionary<string, List<string>>()
                },
                new HeatmapRow
                {
                    Category = "blockers",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["Feb"] = new List<string> { "Blocker 1" }
                    }
                }
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersCornerCellAndMonthHeaders()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        cut.Find(".hm-corner").TextContent.Should().Be("STATUS");

        var colHeaders = cut.FindAll(".hm-col-hdr");
        colHeaders.Should().HaveCount(4);
        colHeaders[0].TextContent.Should().Be("Jan");
        colHeaders[1].TextContent.Should().Be("Feb");
        colHeaders[2].TextContent.Should().Be("Mar");
        // Current month (index 3) should have " ◀ Now"
        colHeaders[3].TextContent.Should().Contain("Apr");
        colHeaders[3].TextContent.Should().Contain("Now");
        colHeaders[3].ClassName.Should().Contain("current");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersFourCategoryRowHeaders()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        rowHeaders.Should().HaveCount(4);

        rowHeaders[0].ClassName.Should().Contain("ship-hdr");
        rowHeaders[0].TextContent.Should().Contain("Shipped");

        rowHeaders[1].ClassName.Should().Contain("prog-hdr");
        rowHeaders[1].TextContent.Should().Contain("In Progress");

        rowHeaders[2].ClassName.Should().Contain("carry-hdr");
        rowHeaders[2].TextContent.Should().Contain("Carryover");

        rowHeaders[3].ClassName.Should().Contain("block-hdr");
        rowHeaders[3].TextContent.Should().Contain("Blockers");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersGridWithCorrectColumnTemplate()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        var grid = cut.Find(".hm-grid");
        var style = grid.GetAttribute("style");
        style.Should().Contain("grid-template-columns: 160px repeat(4, 1fr)");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersTitleText()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        var title = cut.Find(".hm-title");
        title.TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
        title.TextContent.Should().Contain("SHIPPED");
        title.TextContent.Should().Contain("IN PROGRESS");
        title.TextContent.Should().Contain("CARRYOVER");
        title.TextContent.Should().Contain("BLOCKERS");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_WithNullData_RendersEmptyGridWithoutError()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, (HeatmapData)null!));

        // Should render without throwing; empty HeatmapData has 0 months and 0 rows
        cut.Find(".hm-wrap").Should().NotBeNull();
        cut.Find(".hm-corner").TextContent.Should().Be("STATUS");
        cut.FindAll(".hm-col-hdr").Should().BeEmpty();
        cut.FindAll(".hm-row-hdr").Should().BeEmpty();
    }
}