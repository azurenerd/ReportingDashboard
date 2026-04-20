using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Web.Tests.Integration;

[Trait("Category", "Integration")]
public class StaticSsrEnforcementTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StaticSsrEnforcementTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_Returns200_WithHtmlContent()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("<body");
    }

    [Fact]
    public async Task GetRoot_DoesNot_ContainInteractiveBlazorArtifacts()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().NotContain("blazor.server.js");
        body.Should().NotContain("blazor.web.js");
        body.Should().NotContain("components-reconnect-modal");
        body.Should().NotContain("render-mode=");
        body.ToLowerInvariant().Should().NotContain("loading...");
    }

    [Fact]
    public async Task GetRoot_Contains_ExpectedLayoutBandsAndStylesheetLinks()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().Contain("class=\"hdr\"");
        body.Should().Contain("class=\"tl-area\"");
        body.Should().Contain("class=\"hm-wrap\"");
        body.Should().Contain("class=\"hm-grid\"");
        body.Should().Contain("app.css");
        body.Should().Contain("ReportingDashboard.Web.styles.css");
    }

    [Fact]
    public async Task GetAppCss_Returns200_WithBodyDimensionRules()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/app.css");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var css = await resp.Content.ReadAsStringAsync();
        css.Should().Contain("1920px");
        css.Should().Contain("1080px");
    }

    [Fact]
    public async Task GetScopedStylesBundle_Returns200()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/ReportingDashboard.Web.styles.css");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}