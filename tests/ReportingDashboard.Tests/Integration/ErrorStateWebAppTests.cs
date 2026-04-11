using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying application behavior when data.json is missing or malformed.
/// Uses WebApplicationFactory with custom WebRootPath to simulate error conditions.
/// </summary>
[Trait("Category", "Integration")]
public class ErrorStateWebAppTests : IDisposable
{
    private readonly List<string> _tempDirs = new();

    public void Dispose()
    {
        foreach (var dir in _tempDirs)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }

    private string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"ErrWebApp_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        _tempDirs.Add(dir);
        return dir;
    }

    private WebApplicationFactory<ReportingDashboard.Components.App> CreateFactory(string webRootPath)
    {
        return new WebApplicationFactory<ReportingDashboard.Components.App>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("WebRootPath", webRootPath);
            });
    }

    [Fact]
    public async Task MissingDataJson_RootReturns200_WithErrorPanelContent()
    {
        var emptyDir = CreateTempDir();
        // No data.json in this directory
        using var factory = CreateFactory(emptyDir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error-panel", content);
    }

    [Fact]
    public async Task MissingDataJson_ErrorMessageContainsNotFound()
    {
        var emptyDir = CreateTempDir();
        using var factory = CreateFactory(emptyDir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("not found", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MalformedDataJson_RootReturns200_WithErrorPanel()
    {
        var dir = CreateTempDir();
        File.WriteAllText(Path.Combine(dir, "data.json"), "{ this is not valid JSON at all }}}");
        using var factory = CreateFactory(dir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error-panel", content);
    }

    [Fact]
    public async Task MalformedDataJson_ErrorMessageContainsParseInfo()
    {
        var dir = CreateTempDir();
        File.WriteAllText(Path.Combine(dir, "data.json"), "{{{bad json");
        using var factory = CreateFactory(dir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("parse", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmptyDataJson_RootReturns200_WithErrorPanel()
    {
        var dir = CreateTempDir();
        File.WriteAllText(Path.Combine(dir, "data.json"), "");
        using var factory = CreateFactory(dir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error-panel", content);
    }

    [Fact]
    public async Task ValidationErrorDataJson_RootReturns200_WithValidationError()
    {
        var dir = CreateTempDir();
        // Valid JSON but missing required fields
        File.WriteAllText(Path.Combine(dir, "data.json"), "{}");
        using var factory = CreateFactory(dir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("error-panel", content);
    }

    [Fact]
    public async Task MissingDataJson_DoesNotShowDashboardContent()
    {
        var emptyDir = CreateTempDir();
        using var factory = CreateFactory(emptyDir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        // Should not contain dashboard sections when data is missing
        Assert.DoesNotContain("class=\"dashboard\"", content);
    }

    [Fact]
    public async Task MissingDataJson_ErrorPanelShowsStaticTitle()
    {
        var emptyDir = CreateTempDir();
        using var factory = CreateFactory(emptyDir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Dashboard data could not be loaded", content);
    }

    [Fact]
    public async Task MissingDataJson_ErrorPanelShowsHelpText()
    {
        var emptyDir = CreateTempDir();
        using var factory = CreateFactory(emptyDir);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Check data.json for errors and restart the application", content);
    }
}