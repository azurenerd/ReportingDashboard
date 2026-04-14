using System.Net;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class MiddlewarePipelineTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public MiddlewarePipelineTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task BlazorPipeline_RendersHtmlWithBlazorScript()
    {
        var response = await _fixture.Client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("blazor.server.js", html);
    }

    [Fact]
    public async Task StaticFiles_Middleware_ServesCssWithCorrectContentType()
    {
        var response = await _fixture.Client.GetAsync("/css/dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task StaticFiles_Middleware_ServesJsonWithCorrectContentType()
    {
        var response = await _fixture.Client.GetAsync("/data.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task AntiforgeryMiddleware_IsConfigured()
    {
        // UseAntiforgery() is required between routing and endpoints in .NET 8 Blazor.
        // If it's missing, MapRazorComponents would fail at startup.
        // The fact that we get a successful response proves the middleware pipeline is correctly configured.
        var response = await _fixture.Client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}