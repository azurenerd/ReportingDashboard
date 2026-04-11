using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class StaticFileIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public StaticFileIntegrationTests()
    {
        // Use the real content root so we get the actual wwwroot/css/app.css
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAppCss_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/css/app.css");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAppCss_ReturnsCssContentType()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType!.MediaType.Should().Be("text/css");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsRootCustomProperties()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(":root");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsHeaderClasses()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".hdr");
            css.Should().Contain(".sub");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsTimelineClasses()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".tl-area");
            css.Should().Contain(".tl-svg-box");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsHeatmapClasses()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".hm-wrap");
            css.Should().Contain(".hm-grid");
            css.Should().Contain(".hm-cell");
            css.Should().Contain(".hm-row-hdr");
            css.Should().Contain(".hm-col-hdr");
            css.Should().Contain(".hm-corner");
            css.Should().Contain(".hm-title");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsRowColorVariants()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".ship-hdr");
            css.Should().Contain(".ship-cell");
            css.Should().Contain(".prog-hdr");
            css.Should().Contain(".prog-cell");
            css.Should().Contain(".carry-hdr");
            css.Should().Contain(".carry-cell");
            css.Should().Contain(".block-hdr");
            css.Should().Contain(".block-cell");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsItemClass()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".it");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsReconnectModalHide()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".components-reconnect-modal");
            css.Should().Contain("display: none !important");
        }
    }

    [Fact]
    public async Task GetAppCss_ContainsCurrentMonthHighlightClass()
    {
        var response = await _client.GetAsync("/css/app.css");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var css = await response.Content.ReadAsStringAsync();
            css.Should().Contain(".apr-hdr");
        }
    }

    [Fact]
    public async Task GetNonExistentStaticFile_Returns404()
    {
        var response = await _client.GetAsync("/css/nonexistent.css");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}