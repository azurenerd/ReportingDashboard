using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardRazorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly Bunit.TestContext _ctx;

    public DashboardRazorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dash-razor-{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "data");
        Directory.CreateDirectory(_dataDir);

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);

        _ctx = new Bunit.TestContext();
        _ctx.Services.AddSingleton(new DashboardDataService(mockEnv.Object));
    }

    public void Dispose()
    {
        _ctx.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private void WriteJsonFile(string filename, object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllText(Path.Combine(_dataDir, filename), json);
    }

    private object CreateValidData() => new
    {
        schemaVersion = 1,
        header = new { title = "Project Phoenix", subtitle = "Engineering - Core Platform", backlogLink = "https://dev.azure.com/org/project", currentDate = "2026-04-15" },
        timeline = new { startDate = "2026-01-01", endDate = "2026-06-30", tracks = new List<object>() },
        heatmap = new { columns = new[] { "January", "February" }, rows = new List<object>() }
    };

    [Fact]
    public void Dashboard_SuccessfulLoad_RendersPlaceholderWithData()
    {
        WriteJsonFile("data.json", CreateValidData());

        // Dashboard.razor uses OnInitializedAsync which is async. We need to render and
        // wait for async lifecycle to complete. bUnit handles this automatically but
        // the component needs NavigationManager to return a valid URI.
        var nav = _ctx.Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        // FakeNavigationManager starts at http://localhost/ by default, which is fine.

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        // Wait for async state changes to complete
        cut.WaitForState(() => cut.Markup.Contains("placeholder") || cut.Markup.Contains("error-container"), TimeSpan.FromSeconds(5));

        cut.Markup.Should().Contain("Dashboard data loaded successfully");
        cut.Markup.Should().Contain("Title: Project Phoenix");
        cut.Markup.Should().Contain("Schema Version: 1");
        cut.Markup.Should().Contain("Heatmap Columns: 2");
        cut.Find(".placeholder").Should().NotBeNull();
    }

    [Fact]
    public void Dashboard_MissingFile_RendersErrorContainer()
    {
        var nav = _ctx.Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        nav.NavigateTo("http://localhost/?data=nonexistent.json");

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        cut.WaitForState(() => cut.Markup.Contains("error-container"), TimeSpan.FromSeconds(5));

        cut.Find(".error-container").Should().NotBeNull();
        var errorMsg = cut.Find(".error-message").TextContent;
        errorMsg.Should().Contain("nonexistent.json not found");
        errorMsg.Should().Contain("Place your data file at wwwroot/data/nonexistent.json");
        cut.Markup.Should().NotContain("placeholder");
    }

    [Fact]
    public void Dashboard_InvalidFilename_RendersArgumentExceptionMessage()
    {
        var nav = _ctx.Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        nav.NavigateTo("http://localhost/?data=../evil.json");

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        cut.WaitForState(() => cut.Markup.Contains("error-container"), TimeSpan.FromSeconds(5));

        cut.Find(".error-container").Should().NotBeNull();
        var errorMsg = cut.Find(".error-message").TextContent;
        errorMsg.Should().Contain("Path separators and '..' are not allowed");
        cut.Markup.Should().NotContain("Dashboard data loaded successfully");
    }

    [Fact]
    public void Dashboard_MalformedJson_RendersFailedToLoadMessage()
    {
        File.WriteAllText(Path.Combine(_dataDir, "bad.json"), "{ not valid json !!!");

        var nav = _ctx.Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        nav.NavigateTo("http://localhost/?data=bad.json");

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        cut.WaitForState(() => cut.Markup.Contains("error-container"), TimeSpan.FromSeconds(5));

        cut.Find(".error-container").Should().NotBeNull();
        var errorMsg = cut.Find(".error-message").TextContent;
        errorMsg.Should().Contain("Failed to load dashboard data:");
        cut.Markup.Should().NotContain("placeholder");
    }

    [Fact]
    public void Dashboard_InvalidSchemaVersion_RendersUnsupportedVersionMessage()
    {
        var badData = new
        {
            schemaVersion = 5,
            header = new { title = "Test", subtitle = "Sub", backlogLink = "https://example.com", currentDate = "2026-04-01" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-06-30", tracks = new List<object>() },
            heatmap = new { columns = new[] { "Jan" }, rows = new List<object>() }
        };
        WriteJsonFile("data.json", badData);

        var nav = _ctx.Services.GetRequiredService<Bunit.TestDoubles.FakeNavigationManager>();
        // Navigate to root so it loads default data.json
        nav.NavigateTo("http://localhost/");

        var cut = _ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();
        cut.WaitForState(() => cut.Markup.Contains("error-container"), TimeSpan.FromSeconds(5));

        cut.Find(".error-container").Should().NotBeNull();
        var errorMsg = cut.Find(".error-message").TextContent;
        errorMsg.Should().Contain("Unsupported schema version: 5. Expected: 1.");
    }
}