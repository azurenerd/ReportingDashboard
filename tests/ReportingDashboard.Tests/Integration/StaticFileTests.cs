using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class StaticFileTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StaticFileTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DataJson_IsAccessibleViaHttp()
    {
        var response = await _client.GetAsync("/data.json");

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task DataJson_ReturnsValidJsonContent()
    {
        var response = await _client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<DashboardData>(content, options);

        Assert.NotNull(data);
        Assert.Equal("Privacy Automation Release Roadmap", data!.Title);
    }

    [Fact]
    public async Task DataJson_ContainsExpectedTimelineTracks()
    {
        var response = await _client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<DashboardData>(content, options);

        Assert.NotNull(data);
        Assert.Equal(3, data!.Timeline.Tracks.Count);
        Assert.Equal("M1", data.Timeline.Tracks[0].Name);
    }

    [Fact]
    public async Task DataJson_ContainsExpectedHeatmapCategories()
    {
        var response = await _client.GetAsync("/data.json");
        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<DashboardData>(content, options);

        Assert.NotNull(data);
        Assert.NotEmpty(data!.Heatmap.Shipped);
        Assert.NotEmpty(data.Heatmap.InProgress);
        Assert.NotEmpty(data.Heatmap.Carryover);
        Assert.NotEmpty(data.Heatmap.Blockers);
    }

    [Fact]
    public async Task DashboardCss_IsAccessibleViaHttp()
    {
        var response = await _client.GetAsync("/css/dashboard.css");

        response.EnsureSuccessStatusCode();
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task NonExistentStaticFile_Returns404()
    {
        var response = await _client.GetAsync("/nonexistent-file.json");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}