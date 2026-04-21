using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Tests.Components;

public class DashboardComponentTests : TestContext
{
    [Fact]
    public void DashboardHeader_RendersAllElements()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Test Project")
            .Add(p => p.Subtitle, "Test Subtitle")
            .Add(p => p.BacklogUrl, "https://example.com/backlog")
            .Add(p => p.CurrentDate, "2026-04-10"));

        var markup = cut.Markup;

        // Title text
        cut.Find("h1").TextContent.Should().Contain("Test Project");

        // Backlog link
        var link = cut.Find("h1 a");
        link.GetAttribute("href").Should().Be("https://example.com/backlog");
        link.TextContent.Should().Contain("ADO Backlog");

        // Subtitle
        cut.Find(".sub").TextContent.Should().Contain("Test Subtitle");

        // Legend items (4 total: PoC, Production, Checkpoint, Now)
        var legendItems = cut.FindAll(".legend-item");
        legendItems.Count.Should().Be(4);
    }

    [Fact]
    public void DashboardHeader_NowLabel_IncludesMonthYear()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.Subtitle, "Sub")
            .Add(p => p.BacklogUrl, "https://example.com")
            .Add(p => p.CurrentDate, "2026-04-10"));

        var legendItems = cut.FindAll(".legend-item");
        var nowItem = legendItems[3];
        nowItem.TextContent.Should().Contain("Now");
        nowItem.TextContent.Should().Contain("Apr");
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersCorrectCount()
    {
        var items = new List<string> { "Item A", "Item B", "Item C" };

        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ColorClass, "ship")
            .Add(p => p.IsCurrentMonth, false)
            .Add(p => p.IsLastColumn, false));

        var itemDivs = cut.FindAll(".it");
        itemDivs.Count.Should().Be(3);
        itemDivs[0].TextContent.Should().Be("Item A");
        itemDivs[1].TextContent.Should().Be("Item B");
        itemDivs[2].TextContent.Should().Be("Item C");
    }

    [Fact]
    public void HeatmapCell_Empty_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string>())
            .Add(p => p.ColorClass, "prog")
            .Add(p => p.IsCurrentMonth, false)
            .Add(p => p.IsLastColumn, false));

        var dash = cut.Find(".empty-dash");
        dash.Should().NotBeNull();
        dash.TextContent.Should().Contain("\u2014"); // mdash
    }

    [Fact]
    public void HeatmapCell_CurrentMonth_HasCurrentMonthClass()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Test" })
            .Add(p => p.ColorClass, "block")
            .Add(p => p.IsCurrentMonth, true)
            .Add(p => p.IsLastColumn, false));

        var cell = cut.Find(".hm-cell");
        cell.ClassList.Should().Contain("current-month");
        cell.ClassList.Should().Contain("block-cell");
    }

    [Fact]
    public void HeatmapGrid_RendersCorrectColumnHeaders()
    {
        var heatmap = new HeatmapData
        {
            Months = new List<string> { "March", "April", "May" },
            CurrentMonth = "April",
            Categories = new List<HeatmapCategory>
            {
                new()
                {
                    Name = "Shipped",
                    ColorClass = "ship",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["March"] = new List<string> { "Item 1" },
                        ["April"] = new List<string> { "Item 2" },
                        ["May"] = new List<string>()
                    }
                }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Heatmap, heatmap));

        // 3 month column headers
        var colHeaders = cut.FindAll(".hm-col-hdr");
        colHeaders.Count.Should().Be(3);

        // Current month header has highlight class
        var currentMonthHdr = cut.FindAll(".current-month-hdr");
        currentMonthHdr.Count.Should().Be(1);
        currentMonthHdr[0].TextContent.Should().Contain("April");
        currentMonthHdr[0].TextContent.Should().Contain("Now");
    }

    [Fact]
    public void HeatmapGrid_RowHeader_ShowsTotalCount()
    {
        var heatmap = new HeatmapData
        {
            Months = new List<string> { "March", "April" },
            CurrentMonth = "April",
            Categories = new List<HeatmapCategory>
            {
                new()
                {
                    Name = "Shipped",
                    ColorClass = "ship",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["March"] = new List<string> { "A", "B" },
                        ["April"] = new List<string> { "C" }
                    }
                }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Heatmap, heatmap));

        var rowHeader = cut.Find(".hm-row-hdr");
        rowHeader.TextContent.Should().Contain("SHIPPED");
        rowHeader.TextContent.Should().Contain("3"); // total items
    }
}