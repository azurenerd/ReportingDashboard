using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardHeaderRenderingIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public DashboardHeaderRenderingIntegrationTests()
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
    public async Task GetRoot_WithValidData_RendersH1WithTitle()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "My Executive Dashboard"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("<h1>My Executive Dashboard</h1>");
    }

    [Fact]
    public async Task GetRoot_WithValidData_RendersSubtitleInSubDiv()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(subtitle: "Engineering · Platform · Apr 2024"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("class=\"sub\"");
        html.Should().Contain("Engineering · Platform · Apr 2024");
    }

    [Fact]
    public async Task GetRoot_WithValidData_HeaderHasHdrClass()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("class=\"hdr\"");
        html.Should().Contain("class=\"hdr-left\"");
    }

    [Fact]
    public async Task GetRoot_WithSpecialCharactersInTitle_RendersEncoded()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Dashboard <Alpha> & \"Beta\""));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // Blazor HTML-encodes special characters
        html.Should().Contain("&amp;");
        html.Should().Contain("&lt;");
        html.Should().Contain("&gt;");
    }

    [Fact]
    public async Task GetRoot_WithUnicodeTitle_RendersCorrectly()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Dashboard 🚀 Ünïcödé"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Dashboard 🚀 Ünïcödé");
    }

    [Fact]
    public async Task GetRoot_WithLongTitle_RendersCompletely()
    {
        var longTitle = new string('A', 200);
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: longTitle));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain(longTitle);
    }
}