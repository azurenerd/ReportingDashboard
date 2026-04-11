using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using System.Text.Json;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Dashboard.razor conditional rendering logic:
/// - ErrorMessage present → show ErrorPanel
/// - Data present → show dashboard
/// - Neither → show fallback ErrorPanel
/// </summary>
[Trait("Category", "Unit")]
public class DashboardPageConditionalTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public DashboardPageConditionalTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashCondTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
            _disposed = true;
        }
        base.Dispose();
    }

    private DashboardDataService CreateServiceWithState(string? json)
    {
        var svc = new DashboardDataService(_logger);
        if (json == null)
        {
            // Load from nonexistent file to trigger error
            svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
        }
        else
        {
            var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
            File.WriteAllText(path, json);
            svc.LoadAsync(path).GetAwaiter().GetResult();
        }
        return svc;
    }

    private string CreateValidJson() => JsonSerializer.Serialize(new
    {
        title = "Test Dashboard",
        subtitle = "Test Team - April",
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
                new { name = "M1", label = "Core", color = "#4285F4", milestones = new[] { new { date = "2026-03-01", type = "poc", label = "PoC" } } }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Item A" } },
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    [Fact]
    public void Dashboard_WhenErrorMessage_ShowsErrorPanelWithMessage()
    {
        var svc = CreateServiceWithState(null);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("not found", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenErrorMessage_DoesNotShowDashboard()
    {
        var svc = CreateServiceWithState(null);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenValidData_ShowsDashboardContent()
    {
        var svc = CreateServiceWithState(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("dashboard", cut.Markup);
        Assert.DoesNotContain("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenValidData_ShowsHeaderSection()
    {
        var svc = CreateServiceWithState(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hdr", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenValidData_ShowsTimelineArea()
    {
        var svc = CreateServiceWithState(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenValidData_ShowsHeatmapWrap()
    {
        var svc = CreateServiceWithState(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenValidData_PassesDataToHeader()
    {
        var svc = CreateServiceWithState(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Test Dashboard", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenServiceNeverLoaded_ShowsFallbackError()
    {
        // Service created but LoadAsync never called: Data is null, ErrorMessage is null
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // Should show the fallback "not available" message since Data is null and ErrorMessage is empty
        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data is not available", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenMalformedJson_ShowsParseError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{{{invalid json");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("parse", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_WhenValidationFails_ShowsValidationError()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>()
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var svc = CreateServiceWithState(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("validation", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}