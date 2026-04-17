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
                        ["Jan"] = new List<string> { "Feature A" },
                        ["Feb"] = new List<string> { "Feature B" },
                        ["Mar"] = new List<string>(),
                        ["Apr"] = new List<string> { "Feature C", "Feature D" }
                    }
                },
                new HeatmapRow
                {
                    Category = "in-progress",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["Apr"] = new List<string> { "Work Item 1" }
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
                        ["Apr"] = new List<string> { "Blocker 1" }
                    }
                }
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersGridWithCorrectStructure()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        // Title
        var title = cut.Find("div.hm-title");
        title.TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
        title.TextContent.Should().Contain("SHIPPED");
        title.TextContent.Should().Contain("BLOCKERS");

        // Corner cell
        var corner = cut.Find("div.hm-corner");
        corner.TextContent.Should().Be("STATUS");

        // Grid style has correct column template
        var grid = cut.Find("div.hm-grid");
        grid.GetAttribute("style").Should().Contain("160px repeat(4, 1fr)");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersCurrentMonthHeaderWithNowIndicator()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        var headers = cut.FindAll("div.hm-col-hdr");
        headers.Should().HaveCount(4);

        // Non-current months should not have "Now"
        headers[0].TextContent.Should().Be("Jan");
        headers[1].TextContent.Should().Be("Feb");
        headers[2].TextContent.Should().Be("Mar");

        // Current month (index 3 = Apr) should have "◀ Now" and "current" class
        headers[3].TextContent.Should().Contain("Apr");
        headers[3].TextContent.Should().Contain("Now");
        headers[3].ClassName.Should().Contain("current");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_RendersFourCategoryRowHeaders()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, data));

        var rowHeaders = cut.FindAll("div.hm-row-hdr");
        rowHeaders.Should().HaveCount(4);

        // Verify category CSS classes and display names with emoji prefixes
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
    public void Heatmap_WithNullParameter_RendersWithoutThrowing()
    {
        var cut = RenderComponent<Heatmap>(p => p
            .Add(x => x.HeatmapData, (HeatmapData)null!));

        // Should render the wrapper and title without crashing
        var title = cut.Find("div.hm-title");
        title.Should().NotBeNull();

        var corner = cut.Find("div.hm-corner");
        corner.TextContent.Should().Be("STATUS");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapSection_RendersIdenticallyToHeatmap()
    {
        var data = CreateTestHeatmapData();

        var cut = RenderComponent<HeatmapSection>(p => p
            .Add(x => x.Heatmap, data));

        // Verify same structure: title, corner, headers, rows
        cut.Find("div.hm-title").TextContent.Should().Contain("MONTHLY EXECUTION HEATMAP");
        cut.Find("div.hm-corner").TextContent.Should().Be("STATUS");

        var rowHeaders = cut.FindAll("div.hm-row-hdr");
        rowHeaders.Should().HaveCount(4);
        rowHeaders[0].ClassName.Should().Contain("ship-hdr");

        var colHeaders = cut.FindAll("div.hm-col-hdr");
        colHeaders[3].TextContent.Should().Contain("Now");
    }
}