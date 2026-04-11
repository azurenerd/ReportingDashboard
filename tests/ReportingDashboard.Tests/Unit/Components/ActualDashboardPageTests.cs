using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the actual Dashboard.razor page which checks
/// !string.IsNullOrEmpty(DataService.ErrorMessage) for error state
/// and uses "dashboard" CSS class (not "dashboard-root").
/// </summary>
[Trait("Category", "Unit")]
public class ActualDashboardPageTests : TestContext
{
    private readonly string _tempDir;

    public ActualDashboardPageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ActualDashPage_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public new void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        base.Dispose();
    }

    private DashboardDataService CreateLoadedService()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Test Dashboard",
            subtitle = "Team X - April",
            backlogLink = "https://ado.example.com",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Core", color = "#4285F4",
                        milestones = new[] { new { date = "2026-03-01", type = "poc", label = "PoC" } }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Item A" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        });
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateErrorService()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);
        svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateUninitializedService()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        return new DashboardDataService(logger);
    }

    [Fact]
    public void Dashboard_WithValidData_RendersDashboardClass()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("dashboard", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithValidData_RendersHeader()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hdr", cut.Markup);
        Assert.Contains("Test Dashboard", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithValidData_RendersTimelinePlaceholder()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithValidData_RendersHeatmapPlaceholder()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithError_RendersErrorPanel()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithError_DoesNotRenderDashboardContent()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Should not contain header or placeholders when in error state
        Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithError_PassesErrorMessageToPanel()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("not found", cut.Markup);
    }

    [Fact]
    public void Dashboard_Uninitialized_DataIsNull_ShowsFallbackError()
    {
        // When service has not been loaded, Data is null and ErrorMessage is null
        // The Dashboard checks: else if (DataService.Data is not null) ... else ErrorPanel
        var svc = CreateUninitializedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data is not available", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithValidData_TimelinePlaceholderHasGuidanceText()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Timeline placeholder", cut.Markup);
    }

    [Fact]
    public void Dashboard_WithValidData_HeatmapPlaceholderHasGuidanceText()
    {
        var svc = CreateLoadedService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Heatmap placeholder", cut.Markup);
    }
}