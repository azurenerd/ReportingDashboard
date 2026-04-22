using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Unit;

public class DashboardComponentTests : TestContext
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Dashboard_ShowsError_WhenDataServiceReturnsError()
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
    public void Dashboard_RendersHeader_WhenDataIsValid()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetError()).Returns((string?)null);
        mockService.Setup(s => s.GetData()).Returns(CreateSampleData());
        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Test Dashboard");
        cut.Markup.Should().Contain("hdr");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_RendersEmptyDash_WhenNoItems()
    {
        var cut = RenderComponent<ReportingDashboard.Web.Components.HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string>())
            .Add(p => p.ColorClass, "ship")
            .Add(p => p.IsCurrentMonth, false)
            .Add(p => p.IsLastColumn, false));

        var items = cut.FindAll(".it");
        items.Should().HaveCount(1);
        items[0].TextContent.Should().Contain("-");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_RendersItems_WhenItemsProvided()
    {
        var items = new List<string> { "Feature A", "Feature B", "Feature C" };

        var cut = RenderComponent<ReportingDashboard.Web.Components.HeatmapCell>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.ColorClass, "prog")
            .Add(p => p.IsCurrentMonth, false)
            .Add(p => p.IsLastColumn, false));

        var renderedItems = cut.FindAll(".it");
        renderedItems.Should().HaveCount(3);
        renderedItems[0].TextContent.Should().Be("Feature A");
        renderedItems[1].TextContent.Should().Be("Feature B");
        renderedItems[2].TextContent.Should().Be("Feature C");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapCell_AppliesCurrentMonthClass_WhenIsCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Web.Components.HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Item" })
            .Add(p => p.ColorClass, "ship")
            .Add(p => p.IsCurrentMonth, true)
            .Add(p => p.IsLastColumn, false));

        cut.Markup.Should().Contain("apr");
    }

    private static DashboardData CreateSampleData()
    {
        return new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-01",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01",
                Tracks = new List<TimelineTrack>
                {
                    new TimelineTrack
                    {
                        Id = "M1",
                        Label = "Track One",
                        Color = "#0078D4",
                        Milestones = new List<Milestone>
                        {
                            new Milestone { Date = "2026-03-01", Type = "checkpoint", Label = "CP1" }
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
                    new HeatmapCategory
                    {
                        Name = "Shipped",
                        ColorClass = "ship",
                        Items = new Dictionary<string, List<string>>
                        {
                            ["Jan"] = new List<string> { "Feature 1" },
                            ["Apr"] = new List<string> { "Feature 2" }
                        }
                    }
                }
            }
        };
    }
}