using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Unit;

public class DashboardComponentTests : TestContext
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_WhenError_RendersErrorMessage()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetError()).Returns("Test error: file not found");
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.Contains("Test error: file not found", cut.Markup);
        Assert.Contains("error-container", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_WhenDataValid_RendersHeader()
    {
        var data = CreateSampleData();
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns(data);
        mockService.Setup(s => s.GetError()).Returns((string?)null);
        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.Contains("Test Dashboard", cut.Markup);
        Assert.Contains("hdr", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_WhenDataValid_RendersHeatmapGrid()
    {
        var data = CreateSampleData();
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns(data);
        mockService.Setup(s => s.GetError()).Returns((string?)null);
        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
        Assert.Contains("Monthly Execution Heatmap", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_WhenDataValid_RendersTimelineSection()
    {
        var data = CreateSampleData();
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns(data);
        mockService.Setup(s => s.GetError()).Returns((string?)null);
        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
        Assert.Contains("svg", cut.Markup);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_WhenBothNull_RendersNothing()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        mockService.Setup(s => s.GetError()).Returns((string?)null);
        Services.AddSingleton<IDataService>(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.DoesNotContain("error-container", cut.Markup);
    }

    private static DashboardData CreateSampleData() => new()
    {
        Title = "Test Dashboard",
        Subtitle = "Test Subtitle",
        BacklogUrl = "https://example.com",
        CurrentDate = "2026-04-10",
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1",
                    Label = "Track One",
                    Color = "#0078D4",
                    Milestones = new List<Milestone>
                    {
                        new() { Label = "CP1", Date = "2026-02-01", Type = "checkpoint" }
                    }
                }
            }
        },
        Heatmap = new HeatmapData
        {
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
            CurrentMonth = "Apr",
            Categories = new List<HeatmapCategory>
            {
                new()
                {
                    Name = "Shipped",
                    ColorClass = "ship",
                    Items = new Dictionary<string, List<string>>
                    {
                        ["Jan"] = new() { "Item A" },
                        ["Feb"] = new(),
                        ["Mar"] = new() { "Item B", "Item C" },
                        ["Apr"] = new()
                    }
                }
            }
        }
    };
}