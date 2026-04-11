using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using System.Text.Json;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Dashboard.razor conditional rendering paths:
/// 1. Service has error -> ErrorPanel
/// 2. Service has data -> dashboard with Header + placeholders
/// 3. Service has no data and no error -> fallback ErrorPanel
/// </summary>
[Trait("Category", "Unit")]
public class DashboardPageConditionalTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public DashboardPageConditionalTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashCond_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            base.Dispose();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private DashboardDataService CreateServiceWithData()
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            title = "Cond Test",
            subtitle = "Sub",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts));

        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateServiceWithError()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateFreshService()
    {
        // Service never had LoadAsync called - Data is null, IsError is false, ErrorMessage is null
        return new DashboardDataService(_logger);
    }

    [Fact]
    public void Dashboard_ErrorState_ShowsErrorPanel()
    {
        var svc = CreateServiceWithError();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_ShowsSpecificErrorMessage()
    {
        var svc = CreateServiceWithError();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("not found", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_NoDashboardDiv()
    {
        var svc = CreateServiceWithError();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidState_ShowsDashboardDiv()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("class=\"dashboard\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidState_ShowsHeader()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hdr", cut.Markup);
        Assert.Contains("Cond Test", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidState_ShowsTimelineAreaPlaceholder()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
        Assert.Contains("Timeline placeholder", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidState_ShowsHeatmapPlaceholder()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
        Assert.Contains("Heatmap placeholder", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidState_NoErrorPanel()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_NullDataNoError_ShowsFallbackErrorPanel()
    {
        // Fresh service with no load: Data=null, IsError=false, ErrorMessage=null
        var svc = CreateFreshService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data is not available", cut.Markup);
    }
}