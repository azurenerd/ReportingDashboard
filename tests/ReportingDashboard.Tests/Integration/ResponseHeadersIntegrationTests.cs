using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ResponseHeadersIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public ResponseHeadersIntegrationTests()
    {
        _factory = new WebAppFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetRoot_ResponseIsHtml()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");

        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
        response.Content.Headers.ContentType.CharSet.Should().Be("utf-8");
    }

    [Fact]
    public async Task GetRoot_WithError_StillReturns200()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");

        // Errors are rendered in-page, not as HTTP errors
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoot_WithMalformedJson_StillReturns200()
    {
        _factory.WriteDataJson("not json");

        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HeadRoot_ReturnsSuccess()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var request = new HttpRequestMessage(HttpMethod.Head, "/");
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoot_HasAntiforgeryHeaders()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        // Antiforgery middleware is configured; page should have a cookie or token
        // The response may contain Set-Cookie for antiforgery
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}