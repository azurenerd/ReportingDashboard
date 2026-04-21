using Bunit;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests;

public class DashboardRenderTests : TestContext
{
    [Fact]
    public void Dashboard_Renders_WithValidData()
    {
        var data = CreateSampleData();
        var result = new DashboardLoadResult(data, null, DateTimeOffset.Now);
        Services.AddSingleton<IDashboardDataService>(new TestDataService(result));

        var cut = RenderComponent<Dashboard>();

        cut.Find(".hdr h1").TextContent.Should().Contain("Test Project");
        cut.Find(".tl-area svg").Should().NotBeNull();
        cut.Find(".hm-grid").Should().NotBeNull();
        cut.FindAll(".hm-cell").Should().HaveCount(16);
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnError()
    {
        var error = new DashboardLoadError("/test/data.json", "File not found", null, null, "NotFound");
        var result = new DashboardLoadResult(null, error, DateTimeOffset.Now);
        Services.AddSingleton<IDashboardDataService>(new TestDataService(result));

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-banner").TextContent.Should().Contain("Failed to load data.json");
    }

    [Fact]
    public void Dashboard_NoBlazorInteractiveScript()
    {
        var data = CreateSampleData();
        var result = new DashboardLoadResult(data, null, DateTimeOffset.Now);
        Services.AddSingleton<IDashboardDataService>(new TestDataService(result));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("blazor.server.js");
        cut.Markup.Should().NotContain("_framework/blazor");
    }

    private static DashboardData CreateSampleData() => new()
    {
        Project = new Project
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com"
        },
        Timeline = new Timeline
        {
            Start = new DateOnly(2026, 1, 1),
            End = new DateOnly(2026, 6, 30),
            Lanes = new[]
            {
                new TimelineLane
                {
                    Id = "M1", Label = "Test Lane", Color = "#0078D4",
                    Milestones = new[]
                    {
                        new Milestone
                        {
                            Date = new DateOnly(2026, 3, 15),
                            Type = MilestoneType.Poc,
                            Label = "Test PoC"
                        }
                    }
                }
            }
        },
        Heatmap = new Heatmap
        {
            Months = new[] { "Jan", "Feb", "Mar", "Apr" },
            MaxItemsPerCell = 4,
            Rows = new[]
            {
                new HeatmapRow
                {
                    Category = HeatmapCategory.Shipped,
                    Cells = new IReadOnlyList<string>[]
                    {
                        new[] { "Item 1" }, Array.Empty<string>(),
                        Array.Empty<string>(), Array.Empty<string>()
                    }
                },
                new HeatmapRow
                {
                    Category = HeatmapCategory.InProgress,
                    Cells = new IReadOnlyList<string>[]
                    {
                        Array.Empty<string>(), new[] { "Item 2" },
                        Array.Empty<string>(), Array.Empty<string>()
                    }
                },
                new HeatmapRow
                {
                    Category = HeatmapCategory.Carryover,
                    Cells = new IReadOnlyList<string>[]
                    {
                        Array.Empty<string>(), Array.Empty<string>(),
                        new[] { "Item 3" }, Array.Empty<string>()
                    }
                },
                new HeatmapRow
                {
                    Category = HeatmapCategory.Blockers,
                    Cells = new IReadOnlyList<string>[]
                    {
                        Array.Empty<string>(), Array.Empty<string>(),
                        Array.Empty<string>(), new[] { "Item 4" }
                    }
                },
            }
        }
    };

    private class TestDataService : IDashboardDataService
    {
        private readonly DashboardLoadResult _result;
        public TestDataService(DashboardLoadResult result) => _result = result;
        public DashboardLoadResult GetCurrent() => _result;
        public event EventHandler? DataChanged { add { } remove { } }
    }
}