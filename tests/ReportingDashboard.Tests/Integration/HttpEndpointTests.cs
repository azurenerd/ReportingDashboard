using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class HttpEndpointTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public HttpEndpointTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RootEndpoint_ReturnsSuccessStatusCode()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task RootEndpoint_ReturnsHtmlContent()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("<!DOCTYPE html");
    }

    [Fact]
    public async Task RootEndpoint_WithMissingData_StillReturnsHtml()
    {
        var client = _fixture.CreateClientWithMissingData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task RootEndpoint_WithMalformedData_StillReturnsHtml()
    {
        var client = _fixture.CreateClientWithMalformedData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task NonExistentEndpoint_Returns404OrRedirect()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/nonexistent-page-xyz");

        // Blazor may return 200 with its SPA fallback or 404
        ((int)response.StatusCode).Should().BeOneOf(200, 404);
    }
}