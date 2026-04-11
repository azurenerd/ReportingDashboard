using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardDataFlowIntegrationTests : TestContext
{
    private static DashboardData CreateFullData() => new()
    {
        Title = "Privacy Automation Roadmap",
        Subtitle = "Trusted Platform – Privacy – April 2026",
        BacklogLink = "https://dev.azure.com/test",
        CurrentMonth = "Apr",
        Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Id = "M1",
                    Name = "Chatbot",
                    Color = "#0078D4",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-02-15", Label = "Feb 15", Type = "poc" },
                        new() { Date = "2026-04-01", Label = "Apr 1", Type = "production" }
                    }
                },
                new()
                {
                    Id = "M2",
                    Name = "Pipeline",
                    Color = "#00897B",
                    Milestones = new List<MilestoneMarker>
                    {
                        new() { Date = "2026-03-01", Label = "Mar 1", Type = "checkpoint" }
                    }
                }
            }
        },
        Heatmap = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "SDK v2.1", "Pipeline" },
                ["feb"] = new() { "API v3" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "Monitoring" }
            },
            Carryover = new Dictionary<string, List<string>>(),
            Blockers = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "Legal Review" }
            }
        }
    };

    private Mock<DashboardDataService> RegisterMockService(DashboardData data)
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        var mock = new Mock<DashboardDataService>(mockEnv.Object, mockLogger.Object);
        mock.Setup(s => s.IsError).Returns(false);
        mock.Setup(s => s.Data).Returns(data);
        Services.AddSingleton(mock.Object);
        return mock;
    }

    [Fact]
    public void Dashboard_WithValidData_RendersAllThreeSections()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".hdr").Should().HaveCount(1);
        cut.FindAll(".tl-area").Should().HaveCount(1);
        cut.FindAll(".hm-wrap").Should().HaveCount(1);
    }

    [Fact]
    public void Dashboard_WithValidData_TimelineReceivesTrackData()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        // Timeline renders track labels
        cut.Markup.Should().Contain("Chatbot");
        cut.Markup.Should().Contain("Pipeline");
        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("M2");
    }

    [Fact]
    public void Dashboard_WithValidData_TimelineRendersSvg()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        cut.Find("svg").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WithValidData_TimelineRendersMilestoneTypes()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        // poc = gold diamond
        cut.Markup.Should().Contain("#F4B400");
        // production = green diamond
        cut.Markup.Should().Contain("#34A853");
        // checkpoint = circle
        cut.FindAll("circle").Should().NotBeEmpty();
    }

    [Fact]
    public void Dashboard_WithValidData_TimelineRendersNowLine()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("NOW");
        cut.Markup.Should().Contain("#EA4335");
    }

    [Fact]
    public void Dashboard_WithEmptyTimeline_RendersWithoutCrashing()
    {
        var data = CreateFullData();
        data.Timeline.Tracks.Clear();
        RegisterMockService(data);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
        cut.FindAll(".tl-label").Should().BeEmpty();
    }

    [Fact]
    public void Dashboard_WithEmptyHeatmap_RendersWithoutCrashing()
    {
        var data = CreateFullData();
        data.Heatmap = new HeatmapData();
        data.Months = new List<string>();
        RegisterMockService(data);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".hm-wrap").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_WithSingleTrack_RendersCorrectly()
    {
        var data = CreateFullData();
        data.Timeline.Tracks = new List<TimelineTrack>
        {
            new()
            {
                Id = "ONLY",
                Name = "Solo Track",
                Color = "#FF0000",
                Milestones = new List<MilestoneMarker>
                {
                    new() { Date = "2026-03-15", Label = "Mar 15", Type = "production" }
                }
            }
        };
        RegisterMockService(data);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".tl-label").Should().HaveCount(1);
        cut.Markup.Should().Contain("Solo Track");
        cut.Markup.Should().Contain("ONLY");
    }

    [Fact]
    public void Dashboard_MilestoneLabelsFlowFromDataToSvg()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Feb 15");
        cut.Markup.Should().Contain("Apr 1");
        cut.Markup.Should().Contain("Mar 1");
    }

    [Fact]
    public void Dashboard_TrackColorsFlowFromDataToSvg()
    {
        RegisterMockService(CreateFullData());

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("#0078D4");
        cut.Markup.Should().Contain("#00897B");
    }
}