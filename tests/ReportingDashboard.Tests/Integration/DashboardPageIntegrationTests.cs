using System.Net;
using FluentAssertions;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardPageIntegrationTests : IClassFixture<WebAppFactory>, IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public DashboardPageIntegrationTests(WebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public void Dispose() => _client.Dispose();

    [Fact]
    public async Task GetRoot_ReturnsSuccessStatusCode()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoot_ReturnsHtmlContentType()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");

        response.Content.Headers.ContentType!.MediaType.Should().Be("text/html");
    }

    [Fact]
    public async Task GetRoot_WithValidData_RendersProjectTitle()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Executive Dashboard Alpha"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Executive Dashboard Alpha");
    }

    [Fact]
    public async Task GetRoot_WithValidData_RendersSubtitle()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(subtitle: "Cloud Engineering · Platform · Apr 2024"));

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Cloud Engineering · Platform · Apr 2024");
    }

    [Fact]
    public async Task GetRoot_WithValidData_RendersHeaderSection()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("class=\"hdr\"");
    }

    [Fact]
    public async Task GetRoot_WithValidData_RendersTimelinePlaceholder()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("class=\"tl-area\"");
        html.Should().Contain("Timeline placeholder");
        html.Should().Contain("1 track(s) configured");
    }

    [Fact]
    public async Task GetRoot_WithValidData_RendersHeatmapPlaceholder()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("class=\"hm-wrap\"");
        html.Should().Contain("Heatmap placeholder");
        html.Should().Contain("4 categories x 6 months");
    }

    [Fact]
    public async Task GetRoot_WithMissingDataJson_RendersErrorMessage()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Dashboard Error");
        html.Should().Contain("data.json not found at:");
    }

    [Fact]
    public async Task GetRoot_WithMissingDataJson_DoesNotRenderStackTrace()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("System.");
        html.Should().NotContain("StackTrace");
        html.Should().NotContain("at ReportingDashboard");
    }

    [Fact]
    public async Task GetRoot_WithMalformedJson_RendersParseError()
    {
        _factory.WriteDataJson("{ broken }");

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Dashboard Error");
        html.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task GetRoot_WithMissingRequiredField_RendersValidationError()
    {
        var json = """
        {
            "project": { "title": "", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Dashboard Error");
        html.Should().Contain("project.title");
    }

    [Fact]
    public async Task GetRoot_WithInvalidNowPosition_RendersValidationError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 1.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Dashboard Error");
        html.Should().Contain("nowPosition");
    }

    [Fact]
    public async Task GetRoot_WithValidData_DoesNotRenderErrorSection()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("Dashboard Error");
        html.Should().NotContain("dashboard-error");
    }

    [Fact]
    public async Task GetRoot_RendersWithin1920Container()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("width:1920px");
        html.Should().Contain("height:1080px");
        html.Should().Contain("overflow:hidden");
    }

    [Fact]
    public async Task GetRoot_HasNoRenderModeAttribute()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("rendermode");
        html.Should().NotContain("blazor-enhanced-nav");
    }

    [Fact]
    public async Task GetRoot_HasCorrectDoctype()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.TrimStart().Should().StartWith("<!DOCTYPE html>");
    }

    [Fact]
    public async Task GetRoot_HasCorrectViewportMeta()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("width=1920");
    }

    [Fact]
    public async Task GetRoot_HasCssLink()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("css/app.css");
    }

    [Fact]
    public async Task GetRoot_HasCorrectTitle()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("<title>Executive Reporting Dashboard</title>");
    }
}