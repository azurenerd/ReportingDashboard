using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests that exercise the full HTTP pipeline using WebApplicationFactory.
/// Tests cover endpoint responses, static file serving, content types, and error states.
/// </summary>
[Trait("Category", "Integration")]
public class HttpEndpointIntegrationTests : IDisposable
{
    private readonly string _tempWebRoot;

    public HttpEndpointIntegrationTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), $"HttpEndpoint_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempWebRoot);
        Directory.CreateDirectory(Path.Combine(_tempWebRoot, "css"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWebRoot))
            Directory.Delete(_tempWebRoot, recursive: true);
    }

    private WebApplicationFactory<ReportingDashboard.Components.App> CreateFactory(
        string? webRootPath = null,
        string? dataJson = null)
    {
        var root = webRootPath ?? _tempWebRoot;

        if (dataJson is not null)
            File.WriteAllText(Path.Combine(root, "data.json"), dataJson);

        // Write a minimal dashboard.css for static file tests
        var cssPath = Path.Combine(root, "css", "dashboard.css");
        if (!File.Exists(cssPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cssPath)!);
            File.WriteAllText(cssPath, "body { width: 1920px; }");
        }

        return new WebApplicationFactory<ReportingDashboard.Components.App>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("WebRootPath", root);
            });
    }

    #region GET / - Dashboard Page

    [Fact]
    public async Task GetRoot_WithValidData_Returns200()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ReturnsHtml()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("text/html", contentType);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsDashboardTitle()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Integration Test Dashboard", html);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsHeaderSection()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("hdr", html);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsBacklogLink()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("https://dev.azure.com/test/backlog", html);
        Assert.Contains("ADO Backlog", html);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsCssReference()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("css/dashboard.css", html);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsBlazorScript()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("_framework/blazor", html);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsViewportMeta()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("width=1920", html);
    }

    [Fact]
    public async Task GetRoot_WithValidData_ContainsPageTitle()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("<title>", html);
    }

    #endregion

    #region GET / - Error States

    [Fact]
    public async Task GetRoot_WithMissingData_Returns200WithErrorPanel()
    {
        var missingDir = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}");
        Directory.CreateDirectory(missingDir);
        Directory.CreateDirectory(Path.Combine(missingDir, "css"));
        File.WriteAllText(Path.Combine(missingDir, "css", "dashboard.css"), "body{}");

        try
        {
            using var factory = CreateFactory(webRootPath: missingDir);
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/");
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("error-panel", html);
            Assert.Contains("Dashboard data could not be loaded", html);
        }
        finally
        {
            if (Directory.Exists(missingDir))
                Directory.Delete(missingDir, recursive: true);
        }
    }

    [Fact]
    public async Task GetRoot_WithMalformedJson_Returns200WithErrorPanel()
    {
        using var factory = CreateFactory(dataJson: "{ totally broken json {{{}");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("error-panel", html);
    }

    [Fact]
    public async Task GetRoot_WithMalformedJson_ErrorContainsParseMessage()
    {
        using var factory = CreateFactory(dataJson: "not json at all!!!!");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("parse", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetRoot_WithNullJson_Returns200WithErrorPanel()
    {
        using var factory = CreateFactory(dataJson: "null");
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("error-panel", html);
    }

    #endregion

    #region Static Files

    [Fact]
    public async Task GetCss_Returns200()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/css/dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCss_ReturnsCssContentType()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/css/dashboard.css");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("text/css", contentType);
    }

    [Fact]
    public async Task GetCss_ContainsCssContent()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/css/dashboard.css");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("1920px", content);
    }

    [Fact]
    public async Task GetDataJson_Returns200()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/data.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDataJson_ReturnsJsonContentType()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/data.json");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task GetDataJson_ContainsExpectedData()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Integration Test Dashboard", content);
        Assert.Contains("timeline", content);
        Assert.Contains("heatmap", content);
    }

    [Fact]
    public async Task GetNonExistentStaticFile_Returns404()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/css/nonexistent.css");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Non-existent Routes

    [Fact]
    public async Task GetUnknownRoute_ReturnsNotFoundContent()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/this-route-does-not-exist");
        var html = await response.Content.ReadAsStringAsync();

        // Blazor router should handle this with the NotFound template
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("not found", html, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}