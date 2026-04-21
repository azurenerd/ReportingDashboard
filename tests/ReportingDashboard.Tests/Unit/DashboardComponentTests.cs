using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardComponentTests : TestContext
{
    [Fact]
    public void Dashboard_WithError_RendersErrorMessage()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetError()).Returns("Test error message");
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Test error message");
        cut.Markup.Should().Contain("error-container");
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersItemDivs()
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
    }

    [Fact]
    public void HeatmapCell_EmptyItems_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.ColorClass, "prog")
            .Add(x => x.IsCurrentMonth, false)
            .Add(x => x.IsLastColumn, false));

        cut.Markup.Should().Contain("-");
        cut.Find(".empty-dash").Should().NotBeNull();
    }

    [Fact]
    public void HeatmapCell_CurrentMonth_AppliesAprCssClass()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string> { "Test" })
            .Add(x => x.ColorClass, "ship")
            .Add(x => x.IsCurrentMonth, true)
            .Add(x => x.IsLastColumn, false));

        cut.Find(".hm-cell").ClassList.Should().Contain("apr");
    }

    [Fact]
    public void DashboardHeader_RendersTitle_AndBacklogLink()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(x => x.Title, "My Project")
            .Add(x => x.Subtitle, "My Subtitle")
            .Add(x => x.BacklogUrl, "https://dev.azure.com/test")
            .Add(x => x.CurrentDate, "2026-04-15"));

        cut.Find("h1").TextContent.Should().Contain("My Project");
        cut.Find("a").GetAttribute("href").Should().Be("https://dev.azure.com/test");
        cut.Markup.Should().Contain("My Subtitle");
        cut.Markup.Should().Contain("Apr 2026");
    }
}