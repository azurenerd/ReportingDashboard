using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// bUnit component tests for Dashboard.razor header section.
/// Validates title, backlog link, subtitle, legend, error page, and error banner rendering.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardHeaderBunitTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;

    public DashboardHeaderBunitTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DashboardBunit_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DataService CreateDataService(string? jsonContent = null)
    {
        if (jsonContent is not null)
            File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), jsonContent);

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
        return new DataService(envMock.Object);
    }

    private static string CreateValidJson(
        string title = "Test Dashboard",
        string subtitle = "Test Subtitle",
        string backlogUrl = "https://dev.azure.com/test",
        string? nowDateOverride = "2026-04-10")
    {
        var data = new
        {
            schemaVersion = 1,
            title,
            subtitle,
            backlogUrl,
            nowDateOverride,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = new[]
                {
                    new
                    {
                        id = "ws1",
                        name = "Workstream 1",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { label = "M1", date = "2026-03-01", type = "poc", labelPosition = (string?)null }
                        }
                    }
                }
            },
            heatmap = new
            {
                monthColumns = new[] { "Jan", "Feb", "Mar", "Apr" },
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        emoji = "\u2705",
                        cssClass = "ship",
                        months = new[]
                        {
                            new { month = "Jan", items = new[] { "Feature A" } },
                            new { month = "Feb", items = Array.Empty<string>() },
                            new { month = "Mar", items = Array.Empty<string>() },
                            new { month = "Apr", items = Array.Empty<string>() }
                        }
                    },
                    new
                    {
                        name = "In Progress",
                        emoji = "\U0001f6a7",
                        cssClass = "prog",
                        months = new[]
                        {
                            new { month = "Jan", items = Array.Empty<string>() },
                            new { month = "Feb", items = Array.Empty<string>() },
                            new { month = "Mar", items = Array.Empty<string>() },
                            new { month = "Apr", items = Array.Empty<string>() }
                        }
                    },
                    new
                    {
                        name = "Carryover",
                        emoji = "\u26A0\uFE0F",
                        cssClass = "carry",
                        months = new[]
                        {
                            new { month = "Jan", items = Array.Empty<string>() },
                            new { month = "Feb", items = Array.Empty<string>() },
                            new { month = "Mar", items = Array.Empty<string>() },
                            new { month = "Apr", items = Array.Empty<string>() }
                        }
                    },
                    new
                    {
                        name = "Blockers",
                        emoji = "\U0001f6d1",
                        cssClass = "block",
                        months = new[]
                        {
                            new { month = "Jan", items = Array.Empty<string>() },
                            new { month = "Feb", items = Array.Empty<string>() },
                            new { month = "Mar", items = Array.Empty<string>() },
                            new { month = "Apr", items = Array.Empty<string>() }
                        }
                    }
                }
            }
        };
        return JsonSerializer.Serialize(data);
    }

    [Fact]
    public void Header_RendersTitle_FromData()
    {
        using var dataService = CreateDataService(CreateValidJson(title: "My Project Roadmap"));
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(dataService);

        var cut = ctx.RenderComponent<Dashboard>();

        var h1 = cut.Find(".hdr h1");
        h1.TextContent.Should().Contain("My Project Roadmap");
    }

    [Fact]
    public void Header_RendersBacklogLink_WithCorrectHref()
    {
        using var dataService = CreateDataService(CreateValidJson(backlogUrl: "https://dev.azure.com/myorg"));
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(dataService);

        var cut = ctx.RenderComponent<Dashboard>();

        var link = cut.Find(".hdr h1 a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/myorg");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Fact]
    public void Header_RendersSubtitle_FromData()
    {
        using var dataService = CreateDataService(CreateValidJson(subtitle: "Platform Engineering \u00B7 April 2026"));
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(dataService);

        var cut = ctx.RenderComponent<Dashboard>();

        var sub = cut.Find(".sub");
        sub.TextContent.Should().Contain("Platform Engineering");
    }

    [Fact]
    public void Header_RendersLegend_WithFourItems()
    {
        using var dataService = CreateDataService(CreateValidJson());
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(dataService);

        var cut = ctx.RenderComponent<Dashboard>();

        var legendItems = cut.FindAll(".legend .legend-item");
        legendItems.Should().HaveCount(4);

        var texts = legendItems.Select(el => el.TextContent.Trim()).ToList();
        texts.Should().Contain(t => t.Contains("PoC Milestone"));
        texts.Should().Contain(t => t.Contains("Production Release"));
        texts.Should().Contain(t => t.Contains("Checkpoint"));
        texts.Should().Contain(t => t.Contains("Now"));
    }

    [Fact]
    public void Dashboard_ShowsErrorPage_WhenNoData()
    {
        // No data.json written => DataService returns null data with error
        using var dataService = CreateDataService();
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(dataService);

        var cut = ctx.RenderComponent<Dashboard>();

        var errorPage = cut.FindAll(".error-page");
        errorPage.Should().NotBeEmpty();
        cut.Markup.Should().Contain("Dashboard Data Error");
        cut.Markup.Should().Contain("data.json");

        // Header should NOT be rendered
        var headers = cut.FindAll(".hdr");
        headers.Should().BeEmpty();
    }
}