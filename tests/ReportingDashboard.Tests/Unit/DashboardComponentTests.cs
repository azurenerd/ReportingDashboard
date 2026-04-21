using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class DashboardComponentTests : TestContext
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_WithError_RendersErrorMessage()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetError()).Returns("Test error message");
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Dashboard Error");
        cut.Markup.Should().Contain("Test error message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_WithItems_RendersItemDivs()
    {
        var items = new List<string> { "Item A", "Item B", "Item C" };

        var cut = RenderComponent<ReportingDashboard.Web.Components.HeatmapCell>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.ColorClass, "ship")
            .Add(c => c.IsCurrentMonth, false)
            .Add(c => c.IsLastColumn, false));

        var itDivs = cut.FindAll(".it");
        itDivs.Count.Should().Be(3);
        cut.Markup.Should().Contain("Item A");
        cut.Markup.Should().Contain("Item B");
        cut.Markup.Should().Contain("Item C");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_Empty_RendersDash()
    {
        var cut = RenderComponent<ReportingDashboard.Web.Components.HeatmapCell>(p => p
            .Add(c => c.Items, new List<string>())
            .Add(c => c.ColorClass, "prog")
            .Add(c => c.IsCurrentMonth, false)
            .Add(c => c.IsLastColumn, false));

        cut.Markup.Should().Contain("-");
        cut.Find(".empty-dash").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_CurrentMonth_AppliesAprClass()
    {
        var cut = RenderComponent<ReportingDashboard.Web.Components.HeatmapCell>(p => p
            .Add(c => c.Items, new List<string> { "Test" })
            .Add(c => c.ColorClass, "ship")
            .Add(c => c.IsCurrentMonth, true)
            .Add(c => c.IsLastColumn, false));

        cut.Find(".hm-cell").ClassList.Should().Contain("apr");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardHeader_RendersParameters()
    {
        var cut = RenderComponent<ReportingDashboard.Web.Components.DashboardHeader>(p => p
            .Add(c => c.Title, "My Project")
            .Add(c => c.Subtitle, "My Subtitle")
            .Add(c => c.BacklogUrl, "https://dev.azure.com/test")
            .Add(c => c.CurrentDate, "2026-04-15"));

        cut.Markup.Should().Contain("My Project");
        cut.Markup.Should().Contain("My Subtitle");
        cut.Find("a[href='https://dev.azure.com/test']").Should().NotBeNull();
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Apr 2026");
    }
}