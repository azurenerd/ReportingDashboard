using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportingDashboard.Web.Tests;

public class SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SmokeTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_Returns200_WithPlaceholderSections()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");

        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Timeline placeholder");
        html.Should().Contain("Heatmap placeholder");
        html.Should().Contain("(placeholder)");
    }

    [Fact]
    public async Task Home_DoesNotContain_BlazorServerJs_StaticSsrOnly()
    {
        var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/");

        html.Should().NotContain("blazor.server.js",
            "Static SSR must not emit the interactive Blazor Server runtime.");
        html.Should().NotContain("blazor.web.js",
            "Static SSR must not emit the Blazor Web runtime.");
    }

    [Fact]
    public async Task AppCss_IsServed_AndContainsViewportLockingReset()
    {
        var client = _factory.CreateClient();

        var css = await client.GetStringAsync("/app.css");

        css.Should().Contain("width:1920px");
        css.Should().Contain("height:1080px");
        css.Should().Contain("overflow:hidden");
    }

    [Fact]
    public async Task Healthz_Returns_Ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("ok");
    }

    [Fact]
    public async Task DataJson_IsServed_AsApplicationJson()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/data.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }
}