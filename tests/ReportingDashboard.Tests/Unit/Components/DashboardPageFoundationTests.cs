using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageFoundationTests : TestContext
{
    private readonly string _tempDir;
    private readonly DashboardDataService _dataService;

    public DashboardPageFoundationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardPage_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        _dataService = new DashboardDataService(mockLogger.Object);
        Services.AddSingleton(_dataService);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        base.Dispose(disposing);
    }

    private string WriteValidJson(string title = "Test Project Dashboard")
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        var json = $$"""
        {
            "title": "{{title}}",
            "subtitle": "Team A – April 2026",
            "backlogLink": "https://dev.azure.com/test",
            "currentMonth": "Apr",
            "months": ["Jan", "Feb", "Mar", "Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [
                    {
                        "name": "Chatbot",
                        "label": "M1",
                        "color": "#0078D4",
                        "milestones": [
                            { "date": "2026-02-15", "label": "Feb 15", "type": "poc" }
                        ]
                    }
                ]
            },
            "heatmap": {
                "shipped": { "jan": ["Item A"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task Dashboard_WhenIsError_RendersErrorMessage()
    {
        await _dataService.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("not found");
    }

    [Fact]
    public async Task Dashboard_WhenIsError_DoesNotRenderDataTitle()
    {
        await _dataService.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("Data loaded:");
    }

    [Fact]
    public async Task Dashboard_WhenDataValid_RendersTitle()
    {
        var path = WriteValidJson("Test Project Dashboard");
        await _dataService.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Data loaded:");
        cut.Markup.Should().Contain("Test Project Dashboard");
    }

    [Fact]
    public async Task Dashboard_WhenDataValid_DoesNotRenderError()
    {
        var path = WriteValidJson();
        await _dataService.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("Error:");
    }

    [Fact]
    public void Dashboard_WhenNoDataLoaded_RendersNothing()
    {
        // Service has default state: IsError=false, Data=null
        var cut = RenderComponent<Dashboard>();

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public async Task Dashboard_WhenIsError_WithSpecificMessage_MessageIsDisplayed()
    {
        await _dataService.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("data.json not found");
    }
}