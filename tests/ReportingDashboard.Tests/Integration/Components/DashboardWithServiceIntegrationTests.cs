using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardWithServiceIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;

    public DashboardWithServiceIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardCompInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    void IDisposable.Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        base.Dispose();
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private async Task<DashboardDataService> CreateLoadedService(string json)
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);
        var path = WriteJson(json);
        await service.LoadAsync(path);
        return service;
    }

    private static string ValidJson => """
    {
        "title": "Integration Test Dashboard",
        "subtitle": "Test Team - April 2026",
        "backlogLink": "https://dev.azure.com/test",
        "currentMonth": "Apr",
        "months": ["Jan", "Feb", "Mar", "Apr"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [
                {
                    "name": "M1",
                    "label": "Feature A",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-03-01", "type": "poc", "label": "PoC Ready" },
                        { "date": "2026-05-15", "type": "production", "label": "Ship" }
                    ]
                },
                {
                    "name": "M2",
                    "label": "Feature B",
                    "color": "#00897B",
                    "milestones": [
                        { "date": "2026-02-15", "type": "checkpoint", "label": "Review" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": { "jan": ["Item A", "Item B"], "feb": ["Item C"] },
            "inProgress": { "mar": ["Item D"] },
            "carryover": {},
            "blockers": { "apr": ["Blocker 1"] }
        }
    }
    """;

    [Fact]
    public async Task Dashboard_WithValidData_RendersDashboardRoot()
    {
        var service = await CreateLoadedService(ValidJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".dashboard-root").Should().NotBeNull();
    }

    [Fact]
    public async Task Dashboard_WithValidData_DoesNotShowErrorPanel()
    {
        var service = await CreateLoadedService(ValidJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-panel").Should().BeEmpty();
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersTimelineArea()
    {
        var service = await CreateLoadedService(ValidJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".tl-area").Should().NotBeNull();
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersTimelineTrackNames()
    {
        var service = await CreateLoadedService(ValidJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("M1");
        cut.Markup.Should().Contain("M2");
        cut.Markup.Should().Contain("Feature A");
        cut.Markup.Should().Contain("Feature B");
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersSvgWithMilestones()
    {
        var service = await CreateLoadedService(ValidJson);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find("svg").Should().NotBeNull();
        // PoC milestone -> gold polygon
        cut.Markup.Should().Contain("#F4B400");
        // Production milestone -> green polygon
        cut.Markup.Should().Contain("#34A853");
        // NOW line
        cut.Markup.Should().Contain("NOW");
    }

    [Fact]
    public async Task Dashboard_WithMissingFile_ShowsErrorPanel()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        var service = new DashboardDataService(logger.Object);
        await service.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-title").TextContent.Should().Contain("Dashboard data could not be loaded");
        cut.Find(".error-details").TextContent.Should().Contain("not found");
    }

    [Fact]
    public async Task Dashboard_WithMalformedJson_ShowsParseError()
    {
        var service = await CreateLoadedService("{ invalid }}}");
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-details").TextContent.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task Dashboard_WithValidationError_ShowsValidationMessage()
    {
        var json = """
        {
            "currentMonth": "Apr",
            "months": [],
            "timeline": { "tracks": [] }
        }
        """;
        var service = await CreateLoadedService(json);
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find(".error-details").TextContent.Should().Contain("validation");
        cut.Find(".error-details").TextContent.Should().Contain("title is required");
    }

    [Fact]
    public async Task Dashboard_ErrorPanel_ShowsHelpText()
    {
        var service = await CreateLoadedService("{}");
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-help").TextContent.Should().Contain("Check data.json for errors and restart the application.");
    }

    [Fact]
    public async Task Dashboard_WithNullTimeline_OmitsTimelineArea()
    {
        var json = """
        {
            "title": "No Timeline",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var service = await CreateLoadedService(json);
        // Manually null out the timeline after successful load to test conditional rendering
        service.Data!.Timeline = null;
        Services.AddSingleton(service);

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".tl-area").Should().BeEmpty();
        cut.Find(".dashboard-root").Should().NotBeNull();
    }
}