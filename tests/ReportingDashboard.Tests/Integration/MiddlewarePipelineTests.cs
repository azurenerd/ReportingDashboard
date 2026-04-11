using FluentAssertions;
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
    public async Task StaticFilesMiddleware_IsRegistered()
    {
        using var client = _fixture.CreateClientWithValidData();

        // UseStaticFiles is wired: a request for a known static path should not 500
        var response = await client.GetAsync("/css/dashboard.css");
        ((int)response.StatusCode).Should().BeLessThan(500);
    }

    [Fact]
    public async Task AntiforgeryMiddleware_DoesNotBlockGetRequests()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task BlazorServerJs_EndpointExists()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/_framework/blazor.server.js");

        // Blazor framework JS is served by the framework middleware
        ((int)response.StatusCode).Should().BeOneOf(200, 301, 302);
    }
}