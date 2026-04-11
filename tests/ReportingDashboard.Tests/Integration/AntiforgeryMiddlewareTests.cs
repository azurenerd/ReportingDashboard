using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Tests verifying the middleware pipeline is correctly configured:
/// antiforgery, static files, and Blazor endpoint mapping.
/// </summary>
[Trait("Category", "Integration")]
public class AntiforgeryMiddlewareTests : IClassFixture<WebApplicationFactory<ReportingDashboard.Components.App>>
{
    private readonly HttpClient _client;

    public AntiforgeryMiddlewareTests(WebApplicationFactory<ReportingDashboard.Components.App> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootRequest_DoesNotRequireAntiforgeryForGET()
    {
        var response = await _client.GetAsync("/");

        // GET requests should not be blocked by antiforgery
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StaticFiles_ServedBeforeAntiforgery()
    {
        // Static files should be served without antiforgery validation
        var response = await _client.GetAsync("/css/dashboard.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RootPage_ContainsBlazorScriptTag()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("blazor", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPage_ContainsDashboardCssLink()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("dashboard.css", content);
    }

    [Fact]
    public async Task RootPage_HasHtmlDoctype()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.StartsWith("<!DOCTYPE html>", content.TrimStart(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPage_HasViewportMeta()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("width=1920", content);
    }

    [Fact]
    public async Task RootPage_HasCorrectTitle()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("<title>Executive", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPage_HasBaseHref()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("base href", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RootPage_HasHtmlLangAttribute()
    {
        var response = await _client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("lang=\"en\"", content);
    }
}