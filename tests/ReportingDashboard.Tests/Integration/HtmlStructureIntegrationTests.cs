using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class HtmlStructureIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public HtmlStructureIntegrationTests()
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
    public async Task GetRoot_ContainsDoctype()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().StartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task GetRoot_ContainsHtmlLangAttribute()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("<html lang=\"en\">");
    }

    [Fact]
    public async Task GetRoot_ContainsCharsetMeta()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("charset=\"utf-8\"");
    }

    [Fact]
    public async Task GetRoot_ContainsViewportMeta1920()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("width=1920");
    }

    [Fact]
    public async Task GetRoot_ContainsPageTitle()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("<title>Executive Reporting Dashboard</title>");
    }

    [Fact]
    public async Task GetRoot_ContainsCssLink()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("href=\"css/app.css\"");
    }

    [Fact]
    public async Task GetRoot_Contains1920x1080Container()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("width:1920px");
        html.Should().Contain("height:1080px");
        html.Should().Contain("overflow:hidden");
        html.Should().Contain("flex-direction:column");
    }

    [Fact]
    public async Task GetRoot_ContainsSegoeUIFontFamily()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Segoe UI");
    }

    [Fact]
    public async Task GetRoot_DoesNotContainBlazorServerScript()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("blazor.server.js");
        html.Should().NotContain("blazor.web.js");
    }

    [Fact]
    public async Task GetRoot_WithValidData_ThreeSectionsPresent()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("class=\"hdr\"");
        html.Should().Contain("class=\"tl-area\"");
        html.Should().Contain("class=\"hm-wrap\"");
    }

    [Fact]
    public async Task GetRoot_WithValidData_SectionsAreInCorrectOrder()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        var hdrIndex = html.IndexOf("class=\"hdr\"", StringComparison.Ordinal);
        var tlIndex = html.IndexOf("class=\"tl-area\"", StringComparison.Ordinal);
        var hmIndex = html.IndexOf("class=\"hm-wrap\"", StringComparison.Ordinal);

        hdrIndex.Should().BeGreaterThan(-1);
        tlIndex.Should().BeGreaterThan(hdrIndex);
        hmIndex.Should().BeGreaterThan(tlIndex);
    }

    [Fact]
    public async Task GetNonExistentRoute_Returns200WithNotFoundMessage()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/nonexistent-page");
        var html = await response.Content.ReadAsStringAsync();

        // Blazor Router handles 404 internally with a "Page not found" message
        // but still returns HTTP 200 since it's server-rendered
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Page not found");
    }
}