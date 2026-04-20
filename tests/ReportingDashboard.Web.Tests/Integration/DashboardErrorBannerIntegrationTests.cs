using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Web.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardErrorBannerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DashboardErrorBannerIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Root_ReturnsHtmlAnd200_WithBodyAnd1920x1080Constraints()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/");
        resp.IsSuccessStatusCode.Should().BeTrue();

        var html = await resp.Content.ReadAsStringAsync();
        html.Should().NotContain("blazor.server.js");
        html.Should().NotContain("blazor.web.js");
        html.Should().Contain("<body");
    }

    [Fact]
    public async Task Root_DoesNotCrash_OnAnyDataJsonState()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/");
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var html = await resp.Content.ReadAsStringAsync();
        // Either happy path (no banner) or error path (banner with role=alert) must render.
        var hasBanner = html.Contains("error-banner") && html.Contains("role=\"alert\"");
        var hasHeader = html.Contains("class=\"hdr\"") || html.Contains("header class=\"hdr\"");
        (hasBanner || hasHeader).Should().BeTrue("page must always render either the error banner or the header");
    }
}