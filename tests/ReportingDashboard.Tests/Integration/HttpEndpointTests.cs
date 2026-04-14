using System.Net;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class HttpEndpointTests : IClassFixture<WebAppFixture>
{
    private readonly HttpClient _client;

    public HttpEndpointTests(WebAppFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task GET_Root_Returns_200()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_Root_Returns_HtmlContent()
    {
        var response = await _client.GetAsync("/");
        var contentType = response.Content.Headers.ContentType?.MediaType;

        Assert.Equal("text/html", contentType);
    }

    [Fact]
    public async Task GET_StaticFile_DashboardCss_Returns_200()
    {
        var response = await _client.GetAsync("/css/dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GET_DataJson_Returns_200()
    {
        var response = await _client.GetAsync("/data.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}