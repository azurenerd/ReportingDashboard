using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class ComponentRenderTests : TestContext
{
    [Fact]
    public void DashboardHeader_RendersTitle_Subtitle_BacklogLink_And_Legend()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(x => x.Title, "My Project")
            .Add(x => x.Subtitle, "Workstream Alpha")
            .Add(x => x.BacklogLink, "https://dev.azure.com/backlog"));

        cut.Markup.Should().Contain("My Project");
        cut.Markup.Should().Contain("Workstream Alpha");
        cut.Find("a[href='https://dev.azure.com/backlog']").Should().NotBeNull();
        cut.FindAll(".legend-item").Count.Should().Be(4);
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersCorrectCount()
    {
        var items = new List<string> { "Item A", "Item B", "Item C" };

        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ColorClass, "shipped")
            .Add(x => x.IsCurrentMonth, false));

        cut.Markup.Should().Contain("Item A");
        cut.Markup.Should().Contain("Item B");
        cut.Markup.Should().Contain("Item C");
        cut.Find(".cell-count").TextContent.Should().Be("3");
    }

    [Fact]
    public void HeatmapCell_Empty_NoCount()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.ColorClass, "shipped")
            .Add(x => x.IsCurrentMonth, false));

        cut.FindAll(".cell-count").Count.Should().Be(0);
    }

    [Fact]
    public void HeatmapGrid_RendersCorrectColumnHeaders()
    {
        var heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["Jan"] = new() { "Done1" },
                ["Feb"] = new(),
                ["Mar"] = new()
            }
        };

        var cut = RenderComponent<HeatmapGrid>(p => p
            .Add(x => x.Heatmap, heatmap)
            .Add(x => x.Months, new List<string> { "Jan", "Feb", "Mar" })
            .Add(x => x.CurrentMonth, "Feb"));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(3);
        cut.Find(".current-month-hdr").TextContent.Should().Contain("Feb");
        cut.Markup.Should().Contain("SHIPPED");
    }

    [Fact]
    public void Dashboard_LoadsData()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.LoadDashboardDataAsync()).ReturnsAsync(new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Sub",
            BacklogLink = "",
            CurrentMonth = "Apr",
            Months = new List<string> { "Apr" },
            Timeline = new TimelineData(),
            Heatmap = new HeatmapData()
        });

        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Test Dashboard");
    }
}