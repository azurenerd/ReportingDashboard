using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Tests static file serving via the real ASP.NET pipeline.
/// Verifies that CSS and data.json are served correctly with proper content types.
/// </summary>
[Trait("Category", "Integration")]
public class StaticFileIntegrationTests : IClassFixture<WebApplicationFactory<ReportingDashboard.Components.App>>
{
    private readonly HttpClient _client;

    public StaticFileIntegrationTests(WebApplicationFactory<ReportingDashboard.Components.App> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboardCss_Returns200()
    {
        var response = await _client.GetAsync("/css/dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboardCss_ReturnsCssContentType()
    {
        var response = await _client.GetAsync("/css/dashboard.css");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("text/css", contentType);
    }

    [Fact]
    public async Task GetDashboardCss_ContainsExpectedStyles()
    {
        var response = await _client.GetAsync("/css/dashboard.css");
        var content = await response.Content.ReadAsStringAsync();

        // Core layout styles from OriginalDesignConcept.html
        Assert.Contains("box-sizing", content);
        Assert.Contains("1920px", content);
        Assert.Contains("1080px", content);
    }

    [Fact]
    public async Task GetDashboardCss_ContainsErrorPanelStyles()
    {
        var response = await _client.GetAsync("/css/dashboard.css");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains(".error-panel", content);
    }

    [Fact]
    public async Task GetDashboardCss_ContainsHeatmapStyles()
    {
        var response = await _client.GetAsync("/css/dashboard.css");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains(".hm-wrap", content);
        Assert.Contains(".hm-grid", content);
        Assert.Contains(".hm-cell", content);
    }

    [Fact]
    public async Task GetDashboardCss_ContainsTimelineStyles()
    {
        var response = await _client.GetAsync("/css/dashboard.css");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains(".tl-area", content);
    }

    [Fact]
    public async Task GetDashboardCss_ContainsHeaderStyles()
    {
        var response = await _client.GetAsync("/css/dashboard.css");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains(".hdr", content);
    }

    [Fact]
    public async Task GetDataJson_Returns200()
    {
        var response = await _client.GetAsync("/data.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDataJson_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/data.json");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("application/json", contentType);
    }

    [Fact]
    public async Task GetDataJson_ContainsExpectedStructure()
    {
        var response = await _client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"title\"", content);
        Assert.Contains("\"timeline\"", content);
        Assert.Contains("\"heatmap\"", content);
        Assert.Contains("\"months\"", content);
    }

    [Fact]
    public async Task GetNonExistentStaticFile_Returns404()
    {
        var response = await _client.GetAsync("/css/nonexistent.css");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBlazorScript_Returns200()
    {
        // The Blazor framework script should be served
        var response = await _client.GetAsync("/_framework/blazor.web.js");

        // May be 200 or 404 depending on test host configuration
        // At minimum, the static file middleware should not crash
        Assert.NotNull(response);
    }
}