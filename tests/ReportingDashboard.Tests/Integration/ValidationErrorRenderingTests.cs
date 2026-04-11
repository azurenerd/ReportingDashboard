using System.Net;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ValidationErrorRenderingTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public ValidationErrorRenderingTests()
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
    public async Task GetRoot_MissingSubtitle_RendersSubtitleValidationError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("error-container");
        html.Should().Contain("project.subtitle");
    }

    [Fact]
    public async Task GetRoot_MissingCurrentMonth_RendersCurrentMonthError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("project.currentMonth");
    }

    [Fact]
    public async Task GetRoot_EmptyTimelineMonths_RendersTimelineError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": [], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("timeline.months");
    }

    [Fact]
    public async Task GetRoot_NowPositionNegative_RendersNowPositionError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": -0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("nowPosition");
    }

    [Fact]
    public async Task GetRoot_NullHeatmapCategories_RendersHeatmapError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("heatmap.categories");
    }

    [Fact]
    public async Task GetRoot_EmptyJsonObject_RendersValidationError()
    {
        _factory.WriteDataJson("{}");

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("error-container");
    }

    [Fact]
    public async Task GetRoot_NullJsonRoot_RendersDeserializationError()
    {
        _factory.WriteDataJson("null");

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("error-container");
    }

    [Fact]
    public async Task GetRoot_WithError_DoesNotRenderHeaderOrTimeline()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("class=\"hdr\"");
        html.Should().NotContain("class=\"tl-area\"");
        html.Should().NotContain("class=\"hm-wrap\"");
    }

    [Fact]
    public async Task GetRoot_WithError_ErrorContainerHasCorrectCssClasses()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("error-container");
        html.Should().Contain("error-message");
    }

    [Fact]
    public async Task GetRoot_WithValidData_NoErrorContainerRendered()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("error-container");
        html.Should().NotContain("error-message");
    }

    [Fact]
    public async Task GetRoot_MultipleValidationIssues_ReportsFirstOne()
    {
        // Title and subtitle both empty; service validates title first
        var json = """
        {
            "project": { "title": "", "subtitle": "", "backlogUrl": "", "currentMonth": "" },
            "timeline": { "months": [], "nowPosition": 5.0 },
            "tracks": [],
            "heatmap": { "months": [], "categories": null }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("project.title");
    }
}