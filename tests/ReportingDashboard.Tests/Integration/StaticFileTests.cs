using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying data.json is served as a static file
/// with correct content and content type.
/// </summary>
public class StaticFileTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public StaticFileTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DataJson_ServedAsStaticFile_Returns200()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/data.json");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        var data = JsonSerializer.Deserialize<DashboardData>(content, JsonOptions);
        data.Should().NotBeNull();
        data!.Title.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DataJson_ContentType_IsApplicationJson()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/data.json");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DataJson_ContainsExpectedTrackCount()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<DashboardData>(content, JsonOptions);
        data.Should().NotBeNull();
        data!.Timeline.Tracks.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DataJson_ContainsAllHeatmapCategories()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        var data = JsonSerializer.Deserialize<DashboardData>(content, JsonOptions);
        data.Should().NotBeNull();
        data!.Heatmap.Should().NotBeNull();
        data.Heatmap.Shipped.Should().NotBeEmpty();
        data.Heatmap.InProgress.Should().NotBeEmpty();
        data.Heatmap.Blockers.Should().NotBeEmpty();
    }
}