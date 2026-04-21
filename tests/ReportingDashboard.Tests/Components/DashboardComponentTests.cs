using Bunit;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Components;

public class DashboardComponentTests : TestContext
{
    [Fact]
    public void DashboardHeader_RendersTitle()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "My Project")
            .Add(p => p.Subtitle, "Subtitle text")
            .Add(p => p.BacklogLink, "https://example.com"));

        var title = cut.Find("h1");
        Assert.Equal("My Project", title.TextContent);
    }

    [Fact]
    public void DashboardHeader_RendersBacklogLink()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Project")
            .Add(p => p.Subtitle, "")
            .Add(p => p.BacklogLink, "https://example.com/backlog"));

        var link = cut.Find("a.backlog-link");
        Assert.Equal("https://example.com/backlog", link.GetAttribute("href"));
    }

    [Fact]
    public void DashboardHeader_HidesBacklogLink_WhenEmpty()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Project")
            .Add(p => p.Subtitle, "")
            .Add(p => p.BacklogLink, ""));

        Assert.Empty(cut.FindAll("a.backlog-link"));
    }

    [Fact]
    public void DashboardHeader_RendersLegend()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Project")
            .Add(p => p.Subtitle, "")
            .Add(p => p.BacklogLink, ""));

        var legendItems = cut.FindAll(".legend-item");
        Assert.Equal(4, legendItems.Count);
    }

    [Fact]
    public void HeatmapCell_RendersCount()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Item A", "Item B" })
            .Add(p => p.ColorClass, "shipped")
            .Add(p => p.IsCurrentMonth, false));

        var count = cut.Find(".cell-count");
        Assert.Equal("2", count.TextContent);
    }

    [Fact]
    public void HeatmapCell_EmptyItems_NoCount()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string>())
            .Add(p => p.ColorClass, "shipped")
            .Add(p => p.IsCurrentMonth, false));

        Assert.Empty(cut.FindAll(".cell-count"));
    }

    [Fact]
    public void HeatmapCell_CurrentMonth_HasClass()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "X" })
            .Add(p => p.ColorClass, "shipped")
            .Add(p => p.IsCurrentMonth, true));

        Assert.Contains("current-month-cell", cut.Find(".hm-cell").ClassList);
    }

    [Fact]
    public void HeatmapGrid_RendersRowHeaders()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "A", "B" }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Heatmap, heatmap)
            .Add(p => p.Months, new List<string> { "Jan" })
            .Add(p => p.CurrentMonth, "Jan"));

        var headers = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, headers.Count);

        Assert.Contains("SHIPPED (2)", headers[0].TextContent);
    }
}