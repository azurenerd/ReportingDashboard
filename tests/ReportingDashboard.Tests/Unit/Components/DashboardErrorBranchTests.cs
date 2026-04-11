using System.Text.Json;
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
/// Focused tests on Dashboard.razor's conditional rendering logic:
/// - ErrorMessage non-empty → renders ErrorPanel with Message parameter
/// - Data not null and no error → renders dashboard with Header/Timeline/Heatmap
/// - Data null and no error message → renders fallback ErrorPanel
/// Tests the three branches in the @if / else if / else chain.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardErrorBranchTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public DashboardErrorBranchTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashErrBranch_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
            base.Dispose();
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private DashboardDataService CreateServiceWithValidData()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "Test Dashboard",
            subtitle = "Team - April",
            backlogLink = "https://ado.example.com",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "Core", color = "#4285F4", milestones = new[] { new { date = "2026-03-01", type = "poc", label = "PoC" } } } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Item A" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);

        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateServiceWithFileNotFound()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateServiceWithMalformedJson()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ not valid json !!! }");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private DashboardDataService CreateServiceWithValidationError()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>(),
            timeline = new { startDate = "", endDate = "", tracks = Array.Empty<object>() },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);

        var path = Path.Combine(_tempDir, "invalid.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    #region Branch 1: ErrorMessage is non-empty → ErrorPanel

    [Fact]
    public void Dashboard_FileNotFound_ShowsErrorPanel()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_FileNotFound_ErrorPanelContainsNotFoundMessage()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("not found", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_FileNotFound_DoesNotShowDashboardContent()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
    }

    [Fact]
    public void Dashboard_MalformedJson_ShowsErrorPanel()
    {
        var svc = CreateServiceWithMalformedJson();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_MalformedJson_ErrorPanelContainsParseError()
    {
        var svc = CreateServiceWithMalformedJson();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("parse", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_ValidationError_ShowsErrorPanel()
    {
        var svc = CreateServiceWithValidationError();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_ShowsStaticErrorTitle()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_ShowsHelpText()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_DoesNotShowTimeline()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_DoesNotShowHeatmap()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("hm-wrap", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_DoesNotShowHeader()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // The hdr class is only inside the dashboard div, not in error panel
        Assert.DoesNotContain("class=\"hdr\"", cut.Markup);
    }

    #endregion

    #region Branch 2: Data is not null → Dashboard content

    [Fact]
    public void Dashboard_ValidData_ShowsDashboardDiv()
    {
        var svc = CreateServiceWithValidData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("dashboard", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_DoesNotShowErrorPanel()
    {
        var svc = CreateServiceWithValidData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_RendersHeader()
    {
        var svc = CreateServiceWithValidData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Test Dashboard", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_RendersTimelineArea()
    {
        var svc = CreateServiceWithValidData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_RendersHeatmapArea()
    {
        var svc = CreateServiceWithValidData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
    }

    #endregion

    #region Branch 3: Fallback (no error, no data) → ErrorPanel with default message

    [Fact]
    public void Dashboard_FreshService_NeverLoaded_ShowsFallbackErrorPanel()
    {
        // A service that was never loaded: Data is null, ErrorMessage is null/empty
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // The else branch renders ErrorPanel with "Dashboard data is not available."
        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_FreshService_NeverLoaded_ShowsFallbackMessage()
    {
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Dashboard data is not available", cut.Markup);
    }

    [Fact]
    public void Dashboard_FreshService_NeverLoaded_DoesNotShowMainDashboard()
    {
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"hdr\"", cut.Markup);
        Assert.DoesNotContain("tl-area", cut.Markup);
        Assert.DoesNotContain("hm-wrap", cut.Markup);
    }

    #endregion

    #region Error Message Passthrough Accuracy

    [Fact]
    public void Dashboard_ErrorMessageFromService_PassedToErrorPanel()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // The actual service error message should appear in the rendered output
        Assert.NotNull(svc.ErrorMessage);
        Assert.Contains(svc.ErrorMessage!, cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorMessage_RenderedInsideErrorContent()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var errorContent = cut.Find(".error-content");
        Assert.NotNull(errorContent);
        Assert.Contains("not found", errorContent.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Mutual Exclusivity of Branches

    [Fact]
    public void Dashboard_ErrorState_OnlyOneErrorPanelRendered()
    {
        var svc = CreateServiceWithFileNotFound();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var errorPanels = cut.FindAll(".error-panel");
        Assert.Single(errorPanels);
    }

    [Fact]
    public void Dashboard_ValidData_ZeroErrorPanelsRendered()
    {
        var svc = CreateServiceWithValidData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var errorPanels = cut.FindAll(".error-panel");
        Assert.Empty(errorPanels);
    }

    [Fact]
    public void Dashboard_FallbackState_OnlyOneErrorPanelRendered()
    {
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var errorPanels = cut.FindAll(".error-panel");
        Assert.Single(errorPanels);
    }

    #endregion
}