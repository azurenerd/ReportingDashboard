using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the root Components/ErrorPanel.razor rendered through
/// the Dashboard page in various error scenarios. Verifies the error panel
/// correctly displays specific error messages from the DashboardDataService.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorPanelComponentIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public ErrorPanelComponentIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ErrPanel_{Guid.NewGuid():N}");
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

    private DashboardDataService CreateServiceWithError(string errorScenario)
    {
        var svc = new DashboardDataService(_logger);

        switch (errorScenario)
        {
            case "missing":
                svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json")).GetAwaiter().GetResult();
                break;
            case "malformed":
                var malPath = Path.Combine(_tempDir, "malformed.json");
                File.WriteAllText(malPath, "{ invalid json {{{}");
                svc.LoadAsync(malPath).GetAwaiter().GetResult();
                break;
            case "validation":
                var valPath = Path.Combine(_tempDir, "validation.json");
                File.WriteAllText(valPath, System.Text.Json.JsonSerializer.Serialize(new
                {
                    title = "",
                    subtitle = "",
                    months = Array.Empty<string>()
                }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
                svc.LoadAsync(valPath).GetAwaiter().GetResult();
                break;
            case "null":
                var nullPath = Path.Combine(_tempDir, "null.json");
                File.WriteAllText(nullPath, "null");
                svc.LoadAsync(nullPath).GetAwaiter().GetResult();
                break;
        }

        return svc;
    }

    [Fact]
    public void Dashboard_MissingFile_ShowsNotFoundError()
    {
        var svc = CreateServiceWithError("missing");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("not found", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_MalformedJson_ShowsParseError()
    {
        var svc = CreateServiceWithError("malformed");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("parse", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_ValidationError_ShowsFieldNames()
    {
        var svc = CreateServiceWithError("validation");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("title", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_NullDeserialization_ShowsNullError()
    {
        var svc = CreateServiceWithError("null");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("null", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_AllErrorStates_ShowHelpText()
    {
        var scenarios = new[] { "missing", "malformed", "validation", "null" };

        foreach (var scenario in scenarios)
        {
            using var ctx = new Bunit.TestContext();
            var svc = CreateServiceWithError(scenario);
            ctx.Services.AddSingleton(svc);

            var cut = ctx.RenderComponent<Dashboard>();

            Assert.Contains("Check data.json for errors and restart the application", cut.Markup);
        }
    }

    [Fact]
    public void Dashboard_AllErrorStates_DoNotRenderDashboard()
    {
        var scenarios = new[] { "missing", "malformed", "validation", "null" };

        foreach (var scenario in scenarios)
        {
            using var ctx = new Bunit.TestContext();
            var svc = CreateServiceWithError(scenario);
            ctx.Services.AddSingleton(svc);

            var cut = ctx.RenderComponent<Dashboard>();

            Assert.DoesNotContain("class=\"dashboard\"", cut.Markup);
        }
    }

    [Fact]
    public void Dashboard_ErrorPanel_MessageContainsMonospaceStyle()
    {
        var svc = CreateServiceWithError("missing");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Consolas", cut.Markup);
    }
}