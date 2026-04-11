using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardDataActualModelTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    #region DashboardData defaults

    [Fact]
    public void DashboardData_DefaultValues_AllStringsEmpty()
    {
        var data = new DashboardData();

        Assert.Equal(string.Empty, data.Title);
        Assert.Equal(string.Empty, data.Subtitle);
        Assert.Equal(string.Empty, data.BacklogLink);
        Assert.Equal(string.Empty, data.CurrentMonth);
    }

    [Fact]
    public void DashboardData_DefaultValues_CollectionsInitialized()
    {
        var data = new DashboardData();

        Assert.NotNull(data.Months);
        Assert.Empty(data.Months);
        Assert.NotNull(data.Timeline);
        Assert.NotNull(data.Heatmap);
    }

    #endregion

    #region TimelineData defaults

    [Fact]
    public void TimelineData_DefaultValues_AllStringsEmpty()
    {
        var tl = new TimelineData();

        Assert.Equal(string.Empty, tl.StartDate);
        Assert.Equal(string.Empty, tl.EndDate);
        Assert.Equal(string.Empty, tl.NowDate);
    }

    [Fact]
    public void TimelineData_DefaultValues_TracksInitialized()
    {
        var tl = new TimelineData();
        Assert.NotNull(tl.Tracks);
        Assert.Empty(tl.Tracks);
    }

    #endregion

    #region TimelineTrack defaults

    [Fact]
    public void TimelineTrack_DefaultValues_AreCorrect()
    {
        var track = new TimelineTrack();

        Assert.Equal(string.Empty, track.Name);
        Assert.Equal(string.Empty, track.Label);
        Assert.Equal("#999", track.Color);
        Assert.NotNull(track.Milestones);
        Assert.Empty(track.Milestones);
    }

    #endregion

    #region Milestone defaults

    [Fact]
    public void Milestone_DefaultValues_AreCorrect()
    {
        var ms = new Milestone();

        Assert.Equal(string.Empty, ms.Date);
        Assert.Equal("checkpoint", ms.Type);
        Assert.Equal(string.Empty, ms.Label);
    }

    #endregion

    #region HeatmapData defaults

    [Fact]
    public void HeatmapData_DefaultValues_AllDictionariesInitialized()
    {
        var hm = new HeatmapData();

        Assert.NotNull(hm.Shipped);
        Assert.Empty(hm.Shipped);
        Assert.NotNull(hm.InProgress);
        Assert.Empty(hm.InProgress);
        Assert.NotNull(hm.Carryover);
        Assert.Empty(hm.Carryover);
        Assert.NotNull(hm.Blockers);
        Assert.Empty(hm.Blockers);
    }

    #endregion

    #region JSON round-trip

    [Fact]
    public void DashboardData_JsonRoundTrip_PreservesAllFields()
    {
        var original = new DashboardData
        {
            Title = "Project Alpha",
            Subtitle = "Team X - April 2026",
            BacklogLink = "https://dev.azure.com/backlog",
            CurrentMonth = "April",
            Months = new List<string> { "January", "February", "March", "April" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01",
                NowDate = "2026-04-10",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Name = "M1",
                        Label = "Core Platform",
                        Color = "#4285F4",
                        Milestones = new List<Milestone>
                        {
                            new() { Date = "2026-02-15", Type = "poc", Label = "Feb 15" },
                            new() { Date = "2026-05-01", Type = "production", Label = "May 1" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData
            {
                Shipped = new Dictionary<string, List<string>>
                {
                    ["jan"] = new() { "Feature A", "Feature B" }
                },
                InProgress = new Dictionary<string, List<string>>
                {
                    ["apr"] = new() { "Feature C" }
                },
                Carryover = new Dictionary<string, List<string>>(),
                Blockers = new Dictionary<string, List<string>>
                {
                    ["mar"] = new() { "Dependency X" }
                }
            }
        };

        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal("Project Alpha", deserialized!.Title);
        Assert.Equal("Team X - April 2026", deserialized.Subtitle);
        Assert.Equal("https://dev.azure.com/backlog", deserialized.BacklogLink);
        Assert.Equal("April", deserialized.CurrentMonth);
        Assert.Equal(4, deserialized.Months.Count);
        Assert.Equal("2026-01-01", deserialized.Timeline.StartDate);
        Assert.Single(deserialized.Timeline.Tracks);
        Assert.Equal("M1", deserialized.Timeline.Tracks[0].Name);
        Assert.Equal(2, deserialized.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal("poc", deserialized.Timeline.Tracks[0].Milestones[0].Type);
        Assert.Equal(2, deserialized.Heatmap.Shipped["jan"].Count);
        Assert.Single(deserialized.Heatmap.InProgress["apr"]);
        Assert.Single(deserialized.Heatmap.Blockers["mar"]);
    }

    [Fact]
    public void DashboardData_Serialization_UsesJsonPropertyNames()
    {
        var data = new DashboardData
        {
            Title = "Test",
            BacklogLink = "http://link",
            CurrentMonth = "March"
        };

        var json = JsonSerializer.Serialize(data, JsonOptions);

        Assert.Contains("\"title\"", json);
        Assert.Contains("\"backlogLink\"", json);
        Assert.Contains("\"currentMonth\"", json);
        Assert.Contains("\"timeline\"", json);
        Assert.Contains("\"heatmap\"", json);
    }

    [Fact]
    public void HeatmapData_JsonRoundTrip_PreservesDictionaryEntries()
    {
        var hm = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "A" },
                ["feb"] = new() { "B", "C" }
            }
        };

        var json = JsonSerializer.Serialize(hm, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<HeatmapData>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized!.Shipped.Count);
        Assert.Single(deserialized.Shipped["jan"]);
        Assert.Equal(2, deserialized.Shipped["feb"].Count);
    }

    [Fact]
    public void Milestone_JsonRoundTrip_PreservesAllTypes()
    {
        var milestones = new[]
        {
            new Milestone { Date = "2026-01-01", Type = "poc", Label = "PoC" },
            new Milestone { Date = "2026-02-01", Type = "production", Label = "Prod" },
            new Milestone { Date = "2026-03-01", Type = "checkpoint", Label = "Check" }
        };

        var json = JsonSerializer.Serialize(milestones, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Milestone[]>(json, JsonOptions);

        Assert.NotNull(deserialized);
        Assert.Equal(3, deserialized!.Length);
        Assert.Equal("poc", deserialized[0].Type);
        Assert.Equal("production", deserialized[1].Type);
        Assert.Equal("checkpoint", deserialized[2].Type);
    }

    [Fact]
    public void DashboardData_Deserialization_FromEmptyObject_UsesDefaults()
    {
        var json = "{}";
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal(string.Empty, data!.Title);
        Assert.NotNull(data.Months);
        Assert.NotNull(data.Timeline);
        Assert.NotNull(data.Heatmap);
    }

    [Fact]
    public void TimelineTrack_Color_DefaultsToGray()
    {
        var json = "{\"name\":\"T1\",\"label\":\"Track One\"}";
        var track = JsonSerializer.Deserialize<TimelineTrack>(json, JsonOptions);

        Assert.NotNull(track);
        Assert.Equal("#999", track!.Color);
    }

    #endregion
}