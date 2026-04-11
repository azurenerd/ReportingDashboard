using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests that fetch the actual wwwroot/data.json through the HTTP pipeline
/// and verify it deserializes correctly into the model types.
/// This validates the contract between the sample data file and the C# models.
/// </summary>
[Trait("Category", "Integration")]
public class DataJsonContractIntegrationTests : IClassFixture<WebApplicationFactory<ReportingDashboard.Components.App>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DataJsonContractIntegrationTests(WebApplicationFactory<ReportingDashboard.Components.App> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DataJson_DeserializesToDashboardData()
    {
        var response = await _client.GetAsync("/data.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.NotNull(data);
    }

    [Fact]
    public async Task DataJson_HasNonEmptyTitle()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.False(string.IsNullOrWhiteSpace(data!.Title));
    }

    [Fact]
    public async Task DataJson_HasNonEmptySubtitle()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.False(string.IsNullOrWhiteSpace(data!.Subtitle));
    }

    [Fact]
    public async Task DataJson_HasBacklogLink()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.False(string.IsNullOrWhiteSpace(data!.BacklogLink));
        Assert.StartsWith("https://", data.BacklogLink);
    }

    [Fact]
    public async Task DataJson_HasAtLeastFourMonths()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.True(data!.Months.Count >= 4, $"Expected at least 4 months, got {data.Months.Count}");
    }

    [Fact]
    public async Task DataJson_HasCurrentMonth()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.False(string.IsNullOrWhiteSpace(data!.CurrentMonth));
    }

    [Fact]
    public async Task DataJson_TimelineHasValidDates()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        var tl = data!.Timeline;
        Assert.False(string.IsNullOrWhiteSpace(tl.StartDate));
        Assert.False(string.IsNullOrWhiteSpace(tl.EndDate));
        Assert.False(string.IsNullOrWhiteSpace(tl.NowDate));

        // Verify they parse as dates
        Assert.True(DateTime.TryParse(tl.StartDate, out _), $"StartDate '{tl.StartDate}' is not a valid date");
        Assert.True(DateTime.TryParse(tl.EndDate, out _), $"EndDate '{tl.EndDate}' is not a valid date");
        Assert.True(DateTime.TryParse(tl.NowDate, out _), $"NowDate '{tl.NowDate}' is not a valid date");
    }

    [Fact]
    public async Task DataJson_TimelineHasAtLeastThreeTracks()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.True(data!.Timeline.Tracks.Count >= 3,
            $"Expected at least 3 tracks per acceptance criteria, got {data.Timeline.Tracks.Count}");
    }

    [Fact]
    public async Task DataJson_AllTracksHaveRequiredFields()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        foreach (var track in data!.Timeline.Tracks)
        {
            Assert.False(string.IsNullOrWhiteSpace(track.Name), "Track name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(track.Label), "Track label should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(track.Color), "Track color should not be empty");
            Assert.StartsWith("#", track.Color, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task DataJson_TracksHaveMilestones()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        var totalMilestones = data!.Timeline.Tracks.Sum(t => t.Milestones.Count);
        Assert.True(totalMilestones > 0, "Expected at least one milestone across all tracks");
    }

    [Fact]
    public async Task DataJson_MilestonesHaveValidTypes()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        var validTypes = new HashSet<string> { "poc", "production", "checkpoint" };
        foreach (var track in data!.Timeline.Tracks)
        {
            foreach (var ms in track.Milestones)
            {
                Assert.Contains(ms.Type, validTypes);
                Assert.False(string.IsNullOrWhiteSpace(ms.Date), "Milestone date should not be empty");
            }
        }
    }

    [Fact]
    public async Task DataJson_HeatmapHasAllFourCategories()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        Assert.NotNull(data!.Heatmap.Shipped);
        Assert.NotNull(data.Heatmap.InProgress);
        Assert.NotNull(data.Heatmap.Carryover);
        Assert.NotNull(data.Heatmap.Blockers);
    }

    [Fact]
    public async Task DataJson_HeatmapHasItemsInAllCategories()
    {
        var response = await _client.GetAsync("/data.json");
        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        var hm = data!.Heatmap;
        var totalShipped = hm.Shipped.Values.Sum(v => v.Count);
        var totalInProgress = hm.InProgress.Values.Sum(v => v.Count);
        var totalCarryover = hm.Carryover.Values.Sum(v => v.Count);
        var totalBlockers = hm.Blockers.Values.Sum(v => v.Count);

        Assert.True(totalShipped > 0, "Expected shipped items in data.json");
        Assert.True(totalInProgress > 0, "Expected in-progress items in data.json");
        Assert.True(totalCarryover > 0, "Expected carryover items in data.json");
        Assert.True(totalBlockers > 0, "Expected blocker items in data.json");
    }
}