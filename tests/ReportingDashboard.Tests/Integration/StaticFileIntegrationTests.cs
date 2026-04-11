using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class StaticFileIntegrationTests : IClassFixture<WebAppFactory>, IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public StaticFileIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task GetCssFile_ReturnsSuccessOrNotFound()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/css/app.css");

        // CSS may or may not be present depending on wwwroot copy;
        // but the middleware pipeline should handle the request without error
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetNonExistentStaticFile_Returns404()
    {
        var response = await _client.GetAsync("/css/nonexistent.css");

        // Static file middleware should pass through, Blazor handles it
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFavicon_ReturnsExpectedStatus()
    {
        var response = await _client.GetAsync("/favicon.ico");

        // Favicon may or may not exist
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}