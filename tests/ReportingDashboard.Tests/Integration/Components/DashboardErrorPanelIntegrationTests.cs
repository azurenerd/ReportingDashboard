using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class DashboardErrorPanelIntegrationTests : TestContext
{
    private readonly string _tempDir;
    private readonly DashboardDataService _service;

    public DashboardErrorPanelIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardErr_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var mockLogger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(mockLogger.Object);
        Services.AddSingleton(_service);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        base.Dispose(disposing);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task Dashboard_WhenFileNotFound_ShowsNotFoundError()
    {
        await _service.LoadAsync(Path.Combine(_tempDir, "data.json"));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("not found");
    }

    [Fact]
    public async Task Dashboard_WhenJsonParseError_ShowsParseError()
    {
        var path = WriteJson("{ invalid json }");
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().Contain("Error:");
        cut.Markup.Should().Contain("parse");
    }

    [Fact]
    public async Task Dashboard_WhenError_DoesNotShowDataLoaded()
    {
        await _service.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("Data loaded:");
    }

    [Fact]
    public async Task Dashboard_WhenDataValid_DoesNotShowError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Apr"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "Track", "label": "T1", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var path = WriteJson(json);
        await _service.LoadAsync(path);

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("Error:");
        cut.Markup.Should().Contain("Data loaded:");
    }

    [Fact]
    public void Dashboard_WhenDataIsNull_RendersEmptyMarkup()
    {
        // Service default state: IsError=false, Data=null
        var cut = RenderComponent<Dashboard>();

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void ErrorPanel_StandaloneRendersWithMessage()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, "Dashboard data file not found"));

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Find("h2").TextContent.Should().Be("Dashboard data could not be loaded");
        cut.Markup.Should().Contain("Dashboard data file not found");
        cut.Find(".error-hint").TextContent.Should().Contain("Check data.json for errors");
    }

    [Fact]
    public void ErrorPanel_StandaloneRendersWithNullMessage()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.Message, (string?)null));

        cut.Find(".error-panel").Should().NotBeNull();
        var paragraphs = cut.FindAll("p");
        paragraphs.Should().HaveCount(1);
        paragraphs[0].ClassList.Should().Contain("error-hint");
    }
}