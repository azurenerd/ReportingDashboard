using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class HttpEndpointTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public HttpEndpointTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RootEndpoint_WithValidData_Returns200()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task RootEndpoint_WithValidData_ContainsDashboardContainer()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        content.Should().Contain("dashboard-container");
    }

    [Fact]
    public async Task RootEndpoint_WithMissingData_Returns200WithErrorBanner()
    {
        using var client = _fixture.CreateClientWithMissingData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error-banner");
    }

    [Fact]
    public async Task RootEndpoint_WithMalformedJson_Returns200WithErrorBanner()
    {
        using var client = _fixture.CreateClientWithMalformedData();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("error-banner");
    }

    [Fact]
    public async Task RootEndpoint_Response_DoesNotContainCorsHeaders()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.Headers.Should().NotContain(h =>
            h.Key.Equals("Access-Control-Allow-Origin", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RootEndpoint_Response_DoesNotContainAuthHeaders()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.Headers.Should().NotContain(h =>
            h.Key.Equals("WWW-Authenticate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RootEndpoint_ContentType_IsHtml()
    {
        using var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");
    }
}