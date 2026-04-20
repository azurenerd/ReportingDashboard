using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Web.Tests.Integration;

[Trait("Category", "Integration")]
public class AppRazorShellTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AppRazorShellTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_ContainsHeadOutletRenderedTitle()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().Contain("<title>").And.Contain("Reporting Dashboard");
    }

    [Fact]
    public async Task Root_ContainsViewportWidth1920Meta()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().Contain("width=1920");
    }

    [Fact]
    public async Task Root_ContainsBaseHrefRoot()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().Contain("<base href=\"/\"");
    }

    [Fact]
    public async Task Root_DoesNotIncludeBlazorFrameworkScriptTag()
    {
        var client = _factory.CreateClient();
        var body = await client.GetStringAsync("/");

        body.Should().NotContain("_framework/blazor");
    }

    [Fact]
    public async Task UnknownRoute_Returns404()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/this-page-does-not-exist-xyz");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}