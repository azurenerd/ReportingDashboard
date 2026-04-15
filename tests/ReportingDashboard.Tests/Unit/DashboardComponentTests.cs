using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardComponentTests : TestContext
{
    private static DashboardData CreateTestData() => new(
        Header: new ProjectHeader(
            "Project Phoenix",
            "Trusted Platform \u2022 Privacy Automation \u2022 April 2026",
            "https://dev.azure.com/org/project/_backlogs",
            "April 2026"),
        Timeline: new TimelineConfig(
            new DateTime(2026, 1, 1), new DateTime(2026, 12, 31), new DateTime(2026, 4, 15),
            new List<Track>
            {
                new("m1", "M1", "Core API & Auth", "#0078D4"),
                new("m2", "M2", "PDS & Data Inventory", "#00897B"),
                new("m3", "M3", "Auto Review DFD", "#546E7A")
            },
            new List<Milestone>
            {
                new("m1", new DateTime(2026, 3, 26), "PoC", "poc", null),
                new("m2", new DateTime(2026, 5, 15), "Production Release", "production", "Full rollout")
            }
        ),
        Heatmap: new HeatmapData(
            new List<string> { "Jan", "Feb", "Mar", "Apr" }, 3,
            new List<StatusRow>
            {
                new("Shipped", "green", new List<MonthCell>
                {
                    new(new List<string> { "Foundation scaffolding" }),
                    new(new List<string> { "Auth module" }),
                    new(new List<string>()),
                    new(new List<string>())
                }),
                new("In Progress", "blue", new List<MonthCell>
                {
                    new(new List<string>()),
                    new(new List<string> { "Dashboard UI" }),
                    new(new List<string>()),
                    new(new List<string> { "Heatmap grid" })
                })
            }
        )
    );

    [Fact]
    public void WhenDataIsNull_RendersErrorPanel()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        mockService.Setup(s => s.GetError()).Returns("data.json not found at /test/data.json. Create this file with your dashboard data.");

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Pages.Dashboard>();

        cut.Find("h1").TextContent.Should().Be("Dashboard Configuration Error");
        cut.Markup.Should().Contain("data.json not found");
        cut.Markup.Should().Contain("Edit data.json and refresh this page.");
    }

    [Fact]
    public void WhenDataIsValid_RendersPlaceholderWithTitle()
    {
        var data = CreateTestData();
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns(data);
        mockService.Setup(s => s.GetError()).Returns((string?)null);

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Pages.Dashboard>();

        cut.Find("h1").TextContent.Should().Be("Project Phoenix");
        cut.Markup.Should().Contain("Trusted Platform");
        cut.Markup.Should().Contain("data.json loaded successfully");
    }

    [Fact]
    public void ErrorPanel_HasRedHeading()
    {
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        mockService.Setup(s => s.GetError()).Returns("Some error");

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Pages.Dashboard>();

        var h1 = cut.Find("h1");
        h1.GetAttribute("style").Should().Contain("color: #EA4335");
    }

    [Fact]
    public void WhenDataIsValid_HasDashboardPlaceholderClass()
    {
        var data = CreateTestData();
        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns(data);
        mockService.Setup(s => s.GetError()).Returns((string?)null);

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Pages.Dashboard>();

        cut.Find(".dashboard-placeholder").Should().NotBeNull();
    }

    [Fact]
    public void Dispose_UnsubscribesFromOnDataChanged()
    {
        int subscribeCount = 0;
        int unsubscribeCount = 0;

        var mockService = new Mock<IDataService>();
        mockService.Setup(s => s.GetData()).Returns((DashboardData?)null);
        mockService.Setup(s => s.GetError()).Returns("error");
        mockService.SetupAdd(s => s.OnDataChanged += It.IsAny<Action>())
            .Callback<Action>(_ => subscribeCount++);
        mockService.SetupRemove(s => s.OnDataChanged -= It.IsAny<Action>())
            .Callback<Action>(_ => unsubscribeCount++);

        Services.AddSingleton(mockService.Object);

        var cut = RenderComponent<ReportingDashboard.Web.Pages.Dashboard>();

        subscribeCount.Should().Be(1, "component should subscribe to OnDataChanged in OnInitialized");

        DisposeComponents();

        unsubscribeCount.Should().Be(1, "component should unsubscribe from OnDataChanged on Dispose");
    }
}