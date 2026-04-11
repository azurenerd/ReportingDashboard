using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class MiddlewarePipelineTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public MiddlewarePipelineTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task StaticFiles_Middleware_ServesFiles()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/css/dashboard.css");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Antiforgery_Middleware_DoesNotBlock_GetRequests()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task BlazorHub_Endpoint_IsAccessible()
    {
        var client = _fixture.CreateClientWithValidData();

        var response = await client.GetAsync("/_blazor/negotiate");

        // negotiate might return various status codes depending on SignalR config
        // but it should not return 404
        ((int)response.StatusCode).Should().NotBe(404);
    }
}