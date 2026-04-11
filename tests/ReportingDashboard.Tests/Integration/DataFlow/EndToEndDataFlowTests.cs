using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.DataFlow;

/// <summary>
/// Tests the full data flow: JSON file -> DashboardDataService -> Dashboard page -> child components.
/// Uses real DashboardDataService with real file I/O, injected into bUnit rendering.
/// </summary>
[Trait("Category", "Integration")]
public class EndToEndDataFlowTests : TestContext, IDisposable
{
    private readonly string _tempDir;

    public EndToEndDataFlowTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"e2e_data_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public new void Dispose()
    {
        base.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task<DashboardDataService> CreateAndLoadService(string json)
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        await File.WriteAllTextAsync(filePath, json);

        var envMock = new Moq.Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(_tempDir);

        var loggerFactory = LoggerFactory.Create(b => b.AddDebug());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();

        var service = new DashboardDataService(envMock.Object, logger);
        await service.LoadAsync();
        return service;
    }

    private static string CreateCompleteJson() => """
    {
        "title": "E2E Test Dashboard",
        "subtitle": "Integration Test Team · Q2 2026",
        "backlogLink": "https://dev.azure.com/e2e/test",
        "currentMonth": "Mar",
        "months": ["Jan", "Feb", "Mar"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-03-15",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Backend",
                    "color": "#0078D4",
                    "milestones": [
                        { "date": "2026-02-01", "label": "Feb PoC", "type": "poc" },
                        { "date": "2026-05-01", "label": "May GA", "type": "production" }
                    ]
                },
                {
                    "id": "M2",
                    "name": "Frontend",
                    "color": "#34A853",
                    "milestones": [
                        { "date": "2026-03-15", "label": "Check", "type": "checkpoint" }
                    ]
                }
            ]
        },
        "heatmap": {
            "shipped": {
                "Jan": ["Auth Module"],
                "Feb": ["Logging"],
                "Mar": ["Config Service"]
            },
            "inProgress": {
                "Mar": ["Dashboard"]
            },
            "carryover": {
                "Feb": ["Migration"],
                "Mar": ["Migration"]
            },
            "blockers": {
                "Mar": ["Vendor Delay"]
            }
        }
    }
    """;

    [Fact]
    public async Task FullPipeline_JsonToDashboard_AllDataRendered()
    {
        var service = await CreateAndLoadService(CreateCompleteJson());
        Services.AddSingleton(service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Header
        cut.Find("h1").TextContent.Should().Contain("E2E Test Dashboard");
        cut.Find(".sub").TextContent.Should().Contain("Integration Test Team");
        cut.Find("a").GetAttribute("href").Should().Be("https://dev.azure.com/e2e/test");

        // Timeline
        cut.FindAll(".tl-label").Should().HaveCount(2);
        cut.FindAll(".tl-id")[0].TextContent.Should().Be("M1");
        cut.FindAll(".tl-id")[1].TextContent.Should().Be("M2");
        cut.Find("svg").Should().NotBeNull();
        cut.Markup.Should().Contain("Feb PoC");
        cut.Markup.Should().Contain("May GA");
        cut.Markup.Should().Contain("Check");

        // Heatmap
        cut.Find(".hm-title").Should().NotBeNull();
        cut.FindAll(".hm-col-hdr").Should().HaveCount(3);
        cut.Find(".cur-month-hdr").TextContent.Should().Contain("Mar");
        cut.Markup.Should().Contain("Auth Module");
        cut.Markup.Should().Contain("Dashboard");
        cut.Markup.Should().Contain("Vendor Delay");
    }

    [Fact]
    public async Task FullPipeline_FileNotFound_ShowsErrorPanel()
    {
        var envMock = new Moq.Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(_tempDir);
        var loggerFactory = LoggerFactory.Create(b => b.AddDebug());
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        var service = new DashboardDataService(envMock.Object, logger);

        // Don't write any file - data.json missing
        await service.LoadAsync();

        Services.AddSingleton(service);
        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("not found");
        cut.FindAll(".hdr").Should().BeEmpty();
    }

    [Fact]
    public async Task FullPipeline_InvalidJson_ShowsParseError()
    {
        var service = await CreateAndLoadService("{ not valid json !!!}}}");
        Services.AddSingleton(service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find(".error-panel").Should().NotBeNull();
        cut.Markup.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task FullPipeline_MinimalJson_RendersDashboardWithDefaults()
    {
        var service = await CreateAndLoadService("""{ "title": "Minimal Test" }""");
        Services.AddSingleton(service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Find("h1").TextContent.Should().Contain("Minimal Test");
        // No backlog link
        cut.FindAll("a").Should().BeEmpty();
        // Empty timeline (no tracks, model still exists)
        cut.Find(".tl-area").Should().NotBeNull();
        // Heatmap exists but rows are all empty
        cut.Find(".hm-wrap").Should().NotBeNull();
    }

    [Fact]
    public async Task FullPipeline_DataWithSpecialCharacters_ShouldRenderSafely()
    {
        var json = """
        {
            "title": "Dashboard <script>alert('xss')</script>",
            "subtitle": "Team & Partners™ — \"Quoted\"",
            "backlogLink": "https://example.com?q=a&b=c",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-01-15",
                "tracks": [
                    {
                        "id": "M1",
                        "name": "Track with <html>",
                        "color": "#0078D4",
                        "milestones": []
                    }
                ]
            },
            "heatmap": {
                "shipped": { "Jan": ["Item with <b>bold</b>"] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var service = await CreateAndLoadService(json);
        Services.AddSingleton(service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Should render without throwing
        cut.Find("h1").Should().NotBeNull();
        // HTML should be encoded (no raw script execution)
        cut.Markup.Should().NotContain("<script>");
        cut.Markup.Should().NotContain("<b>bold</b>");
    }

    [Fact]
    public async Task FullPipeline_ManyHeatmapItems_ShouldRenderAll()
    {
        var items = string.Join(", ", Enumerable.Range(1, 30).Select(i => $"\"Work Item {i}\""));
        var json = $$"""
        {
            "title": "Busy Dashboard",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": { "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-01-15", "tracks": [] },
            "heatmap": {
                "shipped": { "Jan": [{{items}}] },
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var service = await CreateAndLoadService(json);
        Services.AddSingleton(service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var renderedItems = cut.FindAll(".shipped-dot");
        renderedItems.Should().HaveCount(30);
        cut.Markup.Should().Contain("Work Item 1");
        cut.Markup.Should().Contain("Work Item 30");
    }

    [Fact]
    public async Task FullPipeline_TimelineNowLinePositioning_ShouldBePresent()
    {
        var service = await CreateAndLoadService(CreateCompleteJson());
        Services.AddSingleton(service);

        var cut = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // NOW line should be a dashed red line
        cut.Markup.Should().Contain("stroke=\"#EA4335\"");
        cut.Markup.Should().Contain("stroke-dasharray=\"5,3\"");

        // Milestone shapes present
        cut.FindAll("polygon").Should().NotBeEmpty(); // poc and production diamonds
        cut.FindAll("circle").Should().NotBeEmpty();   // checkpoint circles
    }

    [Fact]
    public async Task FullPipeline_ServiceIsSingleton_DataSharedAcrossRenders()
    {
        var service = await CreateAndLoadService(CreateCompleteJson());
        Services.AddSingleton(service);

        var cut1 = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        var cut2 = RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Both renders should show the same data
        cut1.Find("h1").TextContent.Should().Be(cut2.Find("h1").TextContent);
        cut1.FindAll(".tl-label").Count.Should().Be(cut2.FindAll(".tl-label").Count);
    }
}