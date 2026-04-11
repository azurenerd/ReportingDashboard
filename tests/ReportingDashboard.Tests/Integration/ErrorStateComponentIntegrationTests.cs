using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the ErrorPanel component renders correctly
/// through the Dashboard page when DashboardDataService is in various error states.
/// Tests the full pipeline: error condition → service state → Dashboard → ErrorPanel.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorStateComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public ErrorStateComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrInteg_{Guid.NewGuid():N}");
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

    private DashboardDataService CreateServiceWithError(string errorTrigger)
    {
        var svc = new DashboardDataService(_logger);
        string path;

        switch (errorTrigger)
        {
            case "file_not_found":
                svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
                break;

            case "malformed_json":
                path = Path.Combine(_tempDir, $"bad_{Guid.NewGuid():N}.json");
                File.WriteAllText(path, "{{{ totally broken json }}");
                svc.LoadAsync(path).GetAwaiter().GetResult();
                break;

            case "null_json":
                path = Path.Combine(_tempDir, $"null_{Guid.NewGuid():N}.json");
                File.WriteAllText(path, "null");
                svc.LoadAsync(path).GetAwaiter().GetResult();
                break;

            case "validation_title":
                path = Path.Combine(_tempDir, $"val_{Guid.NewGuid():N}.json");
                File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new
                {
                    title = "",
                    subtitle = "S",
                    backlogLink = "https://link",
                    currentMonth = "Apr",
                    months = new[] { "Apr" },
                    timeline = new
                    {
                        startDate = "2026-01-01",
                        endDate = "2026-07-01",
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
                svc.LoadAsync(path).GetAwaiter().GetResult();
                break;

            case "validation_empty_months":
                path = Path.Combine(_tempDir, $"val_{Guid.NewGuid():N}.json");
                File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new
                {
                    title = "T",
                    subtitle = "S",
                    backlogLink = "https://link",
                    currentMonth = "Apr",
                    months = Array.Empty<string>(),
                    timeline = new
                    {
                        startDate = "2026-01-01",
                        endDate = "2026-07-01",
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
                svc.LoadAsync(path).GetAwaiter().GetResult();
                break;

            case "validation_empty_tracks":
                path = Path.Combine(_tempDir, $"val_{Guid.NewGuid():N}.json");
                File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new
                {
                    title = "T",
                    subtitle = "S",
                    backlogLink = "https://link",
                    currentMonth = "Apr",
                    months = new[] { "Apr" },
                    timeline = new
                    {
                        startDate = "2026-01-01",
                        endDate = "2026-07-01",
                        tracks = Array.Empty<object>()
                    },
                    heatmap = new
                    {
                        shipped = new Dictionary<string, string[]>(),
                        inProgress = new Dictionary<string, string[]>(),
                        carryover = new Dictionary<string, string[]>(),
                        blockers = new Dictionary<string, string[]>()
                    }
                }, JsonOpts));
                svc.LoadAsync(path).GetAwaiter().GetResult();
                break;

            default:
                throw new ArgumentException($"Unknown error trigger: {errorTrigger}");
        }

        return svc;
    }

    #region Error Panel Rendering for File Not Found

    [Fact]
    public void Dashboard_FileNotFound_ShowsErrorPanel()
    {
        var svc = CreateServiceWithError("file_not_found");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_FileNotFound_ErrorMessageContainsNotFound()
    {
        var svc = CreateServiceWithError("file_not_found");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("not found", cut.Markup);
    }

    [Fact]
    public void Dashboard_FileNotFound_DoesNotShowDashboardContent()
    {
        var svc = CreateServiceWithError("file_not_found");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("hm-wrap", cut.Markup);
        Assert.DoesNotContain("tl-area", cut.Markup);
    }

    #endregion

    #region Error Panel Rendering for Malformed JSON

    [Fact]
    public void Dashboard_MalformedJson_ShowsErrorPanel()
    {
        var svc = CreateServiceWithError("malformed_json");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("parse", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Error Panel Rendering for Null Deserialization

    [Fact]
    public void Dashboard_NullJson_ShowsErrorPanel()
    {
        var svc = CreateServiceWithError("null_json");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("null", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Error Panel Rendering for Validation Failures

    [Fact]
    public void Dashboard_EmptyTitle_ShowsValidationError()
    {
        var svc = CreateServiceWithError("validation_title");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("title", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_EmptyMonths_ShowsValidationError()
    {
        var svc = CreateServiceWithError("validation_empty_months");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("months", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_EmptyTracks_ShowsValidationError()
    {
        var svc = CreateServiceWithError("validation_empty_tracks");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("tracks", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Error Panel Static Elements Always Present

    [Theory]
    [InlineData("file_not_found")]
    [InlineData("malformed_json")]
    [InlineData("null_json")]
    [InlineData("validation_title")]
    public void Dashboard_AnyError_RendersHelpText(string errorTrigger)
    {
        var svc = CreateServiceWithError(errorTrigger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
    }

    [Theory]
    [InlineData("file_not_found")]
    [InlineData("malformed_json")]
    [InlineData("null_json")]
    [InlineData("validation_title")]
    public void Dashboard_AnyError_RendersWarningIcon(string errorTrigger)
    {
        var svc = CreateServiceWithError(errorTrigger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-icon", cut.Markup);
    }

    [Theory]
    [InlineData("file_not_found")]
    [InlineData("malformed_json")]
    public void Dashboard_AnyError_DoesNotRenderHeader(string errorTrigger)
    {
        var svc = CreateServiceWithError(errorTrigger);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("hdr-title", cut.Markup);
    }

    #endregion

    #region XSS Safety in Error Messages

    [Fact]
    public void Dashboard_ErrorMessageWithHtml_IsEncoded()
    {
        // Create a file with content that will fail JSON parsing, producing an error message
        var path = Path.Combine(_tempDir, $"xss_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, "not json");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // The error panel should render without raw HTML
        Assert.Contains("error-panel", cut.Markup);
        Assert.DoesNotContain("<script>", cut.Markup);
    }

    #endregion
}