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
            .Add(x => x.BacklogUrl, "https://dev.azure.com/backlog")
            .Add(x => x.CurrentDate, "2026-04-10"));

        cut.Markup.Should().Contain("My Project");
        cut.Markup.Should().Contain("Workstream Alpha");
        cut.Find("a[href='https://dev.azure.com/backlog']").Should().NotBeNull();
        cut.FindAll(".legend-item").Count.Should().Be(4);
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersCorrectCount()
    {
        var items = new List<string> { "Item A", "Item B", "Item C" };

        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ColorClass, "ship")
            .Add(x => x.IsCurrentMonth, false)
            .Add(x => x.IsLastColumn, false));

        cut.FindAll(".it").Count.Should().Be(3);
        cut.Markup.Should().Contain("Item A");
        cut.Markup.Should().Contain("Item B");
        cut.Markup.Should().Contain("Item C");
    }

    [Fact]
    public void HeatmapCell_Empty_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.ColorClass, "prog")
            .Add(x => x.IsCurrentMonth, false)
            .Add(x => x.IsLastColumn, false));

        cut.Find(".empty-dash").Should().NotBeNull();
        cut.FindAll(".it").Count.Should().Be(0);
    }

    [Fact]
    public void HeatmapGrid_RendersCorrectColumnHeaders()
    {
        var heatmap = new HeatmapData
        {
            Months = new List<string> { "Jan", "Feb", "Mar" },
            CurrentMonth = "Feb",
            Categories = new List<HeatmapCategory>
            {
                new()
                {
                    Name = "Shipped",
                    ColorClass = "ship",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["Jan"] = new() { "Done1" },
                        ["Feb"] = new(),
                        ["Mar"] = new()
                    }
                }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(p => p
            .Add(x => x.Heatmap, heatmap));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(3);
        cut.Find(".current-month-hdr").TextContent.Should().Contain("Feb");
        cut.Markup.Should().Contain("SHIPPED");
    }

    [Fact]
    public void Dashboard_WithError_RendersErrorMessage()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        mockService.Setup(s => s.GetError()).Returns("Test error: file not found");

        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Dashboard Error");
        cut.Markup.Should().Contain("Test error: file not found");
    }
}