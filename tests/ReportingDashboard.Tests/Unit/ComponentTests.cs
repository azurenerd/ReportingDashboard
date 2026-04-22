using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class HeaderComponentTests : TestContext
{
    [Fact]
    public void Header_RendersProjectTitleAndSubtitle()
    {
        var project = new ProjectInfo
        {
            Title = "My Dashboard",
            Subtitle = "Team · April 2026",
            BacklogUrl = "https://dev.azure.com/test",
            CurrentDate = "2026-04-15"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        cut.Find("h1").TextContent.Should().Contain("My Dashboard");
        cut.Find(".sub").TextContent.Should().Contain("Team · April 2026");
        var link = cut.Find("h1 a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/test");
        link.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void Header_OmitsLinkWhenBacklogUrlIsNull()
    {
        var project = new ProjectInfo
        {
            Title = "No Link Project",
            Subtitle = "Sub"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Project, project));

        cut.Find("h1").TextContent.Should().Contain("No Link Project");
        cut.FindAll("h1 a").Should().BeEmpty();
    }
}

[Trait("Category", "Unit")]
public class HeatmapCellComponentTests : TestContext
{
    [Fact]
    public void HeatmapCell_WithItems_RendersCorrectCount()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };

        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p =>
            p.Add(x => x.Items, items)
             .Add(x => x.CssClass, "ship-cell")
             .Add(x => x.IsHighlighted, false)
             .Add(x => x.IsLastColumn, false));

        var divs = cut.FindAll(".it");
        divs.Should().HaveCount(3);
        divs[0].TextContent.Should().Be("Feature A");
        divs[1].TextContent.Should().Be("Feature B");
        divs[2].TextContent.Should().Be("Feature C");
    }

    [Fact]
    public void HeatmapCell_Empty_RendersDash()
    {
        var cut = RenderComponent<ReportingDashboard.Components.HeatmapCell>(p =>
            p.Add(x => x.Items, (List<string>?)null)
             .Add(x => x.CssClass, "ship-cell")
             .Add(x => x.IsHighlighted, false)
             .Add(x => x.IsLastColumn, false));

        var emptyDiv = cut.Find(".it.empty");
        emptyDiv.TextContent.Should().Be("-");
    }
}

[Trait("Category", "Unit")]
public class DashboardPageErrorStateTests : TestContext
{
    [Fact]
    public void Dashboard_WhenServiceReturnsError_ShowsErrorPanel()
    {
        var mockService = new Mock<IDashboardDataService>();
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        mockService.Setup(s => s.GetError()).Returns("Dashboard data file not found. Expected location: /path/to/file.json");

        Services.AddSingleton<IDashboardDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var errorPanel = cut.Find(".error-panel");
        errorPanel.Should().NotBeNull();
        cut.Find(".error-message").TextContent.Should().Contain("not found");
        cut.Find(".error-hint").TextContent.Should().Contain("Fix the file");
    }
}