using System.Net;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class ErrorRenderingIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;
    private readonly HttpClient _client;

    public ErrorRenderingIntegrationTests()
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
    public async Task MissingFile_RendersErrorDiv_NotBlazorErrorBoundary()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("dashboard-error");
        html.Should().NotContain("blazor-error-boundary");
        html.Should().NotContain("blazor-error-ui");
    }

    [Fact]
    public async Task MissingFile_ErrorContainsFilePath()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("data.json not found at:");
        html.Should().Contain("Data");
    }

    [Fact]
    public async Task MalformedJson_EmptyObject_RendersValidationError()
    {
        _factory.WriteDataJson("{}");

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Dashboard Error");
    }

    [Fact]
    public async Task MalformedJson_ArrayInsteadOfObject_RendersError()
    {
        _factory.WriteDataJson("[1,2,3]");

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Dashboard Error");
    }

    [Fact]
    public async Task MalformedJson_TruncatedString_RendersParseError()
    {
        _factory.WriteDataJson("{\"project\": {\"title\": \"unterminated");

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Dashboard Error");
        html.Should().Contain("Invalid JSON");
    }

    [Fact]
    public async Task ValidationError_EmptyTitle_ShowsFieldName()
    {
        var json = """
        {
            "project": { "title": "", "subtitle": "Sub", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("project.title");
    }

    [Fact]
    public async Task ValidationError_EmptySubtitle_ShowsFieldName()
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

        html.Should().Contain("project.subtitle");
    }

    [Fact]
    public async Task ValidationError_EmptyTimelineMonths_ShowsFieldName()
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
    public async Task ValidationError_InvalidCssClass_ShowsFieldName()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [],
            "heatmap": {
                "months": ["Jan"],
                "categories": [
                    { "name": "X", "cssClass": "invalid", "emoji": "✅", "items": {} }
                ]
            }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Invalid cssClass");
        html.Should().Contain("heatmap.categories[0]");
    }

    [Fact]
    public async Task ValidationError_InvalidMilestoneType_ShowsError()
    {
        var json = """
        {
            "project": { "title": "T", "subtitle": "S", "backlogUrl": "", "currentMonth": "Jan" },
            "timeline": { "months": ["Jan"], "nowPosition": 0.5 },
            "tracks": [
                {
                    "id": "m1", "label": "L", "color": "#000",
                    "milestones": [
                        { "date": "2024-01-01", "type": "invalid", "position": 0.5 }
                    ]
                }
            ],
            "heatmap": { "months": ["Jan"], "categories": [] }
        }
        """;
        _factory.WriteDataJson(json);

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("Invalid milestone type");
    }

    [Fact]
    public async Task ErrorPage_StillReturns200StatusCode()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");

        // Blazor renders error within the page, still returns 200
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ErrorPage_DoesNotRenderDataSections()
    {
        _factory.DeleteDataJson();

        var response = await _client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        html.Should().NotContain("class=\"hdr\"");
        html.Should().NotContain("class=\"tl-area\"");
        html.Should().NotContain("class=\"hm-wrap\"");
    }
}