using System.Text.Json;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the wiring between DashboardDataService error states
/// and the root ErrorPanel.razor component (Components/ErrorPanel.razor) which uses
/// the 'Message' parameter. These tests cover the full pipeline:
/// file on disk → DashboardDataService.LoadAsync → Dashboard.razor conditional → ErrorPanel rendering.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelDashboardWiringTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public ErrorPanelDashboardWiringTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrPanelWiring_{Guid.NewGuid():N}");
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

    private string WriteValidJson()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "Wiring Test Dashboard",
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
        return path;
    }

    #region File Not Found → ErrorPanel Wiring

    [Fact]
    public void Dashboard_FileNotFound_ErrorPanelShowsServiceErrorMessage()
    {
        var missingPath = Path.Combine(_tempDir, "nonexistent.json");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(missingPath).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // ErrorPanel should be rendered with the service's exact error message
        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("not found", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(missingPath, cut.Markup);
    }

    [Fact]
    public void Dashboard_FileNotFound_ErrorPanelContainsStaticTitle()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "missing.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_FileNotFound_ErrorPanelContainsHelpText()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "missing.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Fact]
    public void Dashboard_FileNotFound_NoDashboardContentRendered()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "missing.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
        Assert.DoesNotContain("tl-area", cut.Markup);
        Assert.DoesNotContain("hm-wrap", cut.Markup);
    }

    #endregion

    #region Malformed JSON → ErrorPanel Wiring

    [Fact]
    public void Dashboard_MalformedJson_ErrorPanelShowsParseError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ totally broken json !!! }}}");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("parse", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_EmptyFile_ErrorPanelRendered()
    {
        var path = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(path, "");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_NullJson_ErrorPanelRendered()
    {
        var path = Path.Combine(_tempDir, "null.json");
        File.WriteAllText(path, "null");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void Dashboard_TruncatedJson_ErrorPanelRendered()
    {
        var path = Path.Combine(_tempDir, "truncated.json");
        File.WriteAllText(path, "{\"title\":\"test\"");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
    }

    #endregion

    #region Validation Errors → ErrorPanel Wiring

    [Fact]
    public void Dashboard_EmptyRequiredFields_ErrorPanelShowsValidationMessage()
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
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        // Validation error message should mention specific field
        Assert.Contains("title", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Valid Data → No ErrorPanel

    [Fact]
    public void Dashboard_ValidData_NoErrorPanelRendered()
    {
        var path = WriteValidJson();
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("Dashboard data could not be loaded", cut.Markup);
        var errorPanels = cut.FindAll(".error-panel");
        Assert.Empty(errorPanels);
    }

    [Fact]
    public void Dashboard_ValidData_DashboardContentRendered()
    {
        var path = WriteValidJson();
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Wiring Test Dashboard", cut.Markup);
        Assert.Contains("tl-area", cut.Markup);
        Assert.Contains("hm-wrap", cut.Markup);
    }

    #endregion

    #region Error Message Fidelity Through Pipeline

    [Fact]
    public void Dashboard_FileNotFound_ExactServiceMessageAppearsInErrorPanel()
    {
        var missingPath = Path.Combine(_tempDir, "specific_file.json");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(missingPath).GetAwaiter().GetResult();

        // Capture the exact error message the service produces
        var serviceErrorMessage = svc.ErrorMessage!;
        Assert.NotNull(serviceErrorMessage);

        Services.AddSingleton(svc);
        var cut = RenderComponent<Dashboard>();

        // The exact same message should appear in the rendered ErrorPanel
        Assert.Contains(serviceErrorMessage, cut.Markup);
    }

    [Fact]
    public void Dashboard_ParseError_ExactServiceMessageAppearsInErrorPanel()
    {
        var path = Path.Combine(_tempDir, "parse_err.json");
        File.WriteAllText(path, "{{{{invalid}}}}");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();

        var serviceErrorMessage = svc.ErrorMessage!;
        Assert.NotNull(serviceErrorMessage);

        Services.AddSingleton(svc);
        var cut = RenderComponent<Dashboard>();

        // HTML encoding may change < > & but the text content should match
        var errorContent = cut.Find(".error-content");
        Assert.Contains("parse", errorContent.TextContent, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Fallback Branch (Never Loaded)

    [Fact]
    public void Dashboard_NeverLoadedService_ShowsFallbackErrorPanel()
    {
        var svc = new DashboardDataService(_logger);
        // Never call LoadAsync — Data is null, ErrorMessage is null
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data is not available", cut.Markup);
    }

    [Fact]
    public void Dashboard_NeverLoadedService_FallbackShowsStaticTitle()
    {
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_NeverLoadedService_FallbackShowsHelpText()
    {
        var svc = new DashboardDataService(_logger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    #endregion

    #region ErrorPanel Structure Preserved Through Integration

    [Fact]
    public void Dashboard_ErrorState_ErrorPanelHasErrorContentDiv()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "gone.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var content = cut.Find(".error-content");
        Assert.NotNull(content);
    }

    [Fact]
    public void Dashboard_ErrorState_ErrorPanelHasRedIndicator()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "gone.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("#EA4335", cut.Markup);
        Assert.Contains("48px", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_ErrorPanelHasMonospaceMessage()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "gone.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("monospace", cut.Markup);
    }

    #endregion

    #region No Forbidden Content in Error State

    [Fact]
    public void Dashboard_ErrorState_NoStackTraceExposed()
    {
        var path = Path.Combine(_tempDir, "bad2.json");
        File.WriteAllText(path, "not json at all!!!");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("at System.", cut.Markup);
        Assert.DoesNotContain("StackTrace", cut.Markup);
        Assert.DoesNotContain("Exception", cut.Markup);
    }

    [Fact]
    public void Dashboard_ErrorState_NoBlazorFrameworkDetails()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "nope.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("blazor-error", cut.Markup);
        Assert.DoesNotContain("ComponentBase", cut.Markup);
        Assert.DoesNotContain("RenderTreeBuilder", cut.Markup);
    }

    #endregion
}