using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// End-to-end integration tests verifying the full data pipeline:
/// data.json file → DashboardDataService → DashboardData model.
/// Tests realistic data scenarios including large datasets, edge cases in
/// date ranges, and the complete model structure consistency.
/// </summary>
[Trait("Category", "Integration")]
public class EndToEndDataPipelineTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public EndToEndDataPipelineTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"E2EPipe_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(object data)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    #region Realistic Full Data Scenarios

    [Fact]
    public async Task LoadAsync_RealisticDashboard_AllSectionsComplete()
    {
        var data = new
        {
            title = "Project Phoenix - Executive Dashboard",
            subtitle = "Cloud Platform Team - Sprint 12 - April 2026",
            backlogLink = "https://dev.azure.com/contoso/phoenix/_backlogs/backlog/Cloud%20Platform/Stories",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April", "May", "June" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1", label = "Core Services", color = "#4285F4",
                        milestones = new[]
                        {
                            new { date = "2026-01-15", type = "checkpoint", label = "Jan 15 Design Review" },
                            new { date = "2026-02-28", type = "poc", label = "Feb 28 PoC Complete" },
                            new { date = "2026-05-15", type = "production", label = "May 15 GA" }
                        }
                    },
                    new
                    {
                        name = "M2", label = "Data Platform", color = "#EA4335",
                        milestones = new[]
                        {
                            new { date = "2026-03-01", type = "poc", label = "Mar 1 PoC" },
                            new { date = "2026-06-01", type = "production", label = "Jun 1 Launch" }
                        }
                    },
                    new
                    {
                        name = "M3", label = "Developer Experience", color = "#34A853",
                        milestones = new[]
                        {
                            new { date = "2026-02-01", type = "checkpoint", label = "Feb 1 Review" },
                            new { date = "2026-04-01", type = "poc", label = "Apr 1 PoC" },
                            new { date = "2026-06-15", type = "production", label = "Jun 15 GA" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["january"] = new[] { "Auth Service v2", "Monitoring Dashboard" },
                    ["february"] = new[] { "CI/CD Pipeline", "API Gateway v1" },
                    ["march"] = new[] { "Search Service", "Logging Framework" },
                    ["april"] = new[] { "Cache Layer" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["march"] = new[] { "Data Lake Integration" },
                    ["april"] = new[] { "Analytics Engine", "Export API", "Mobile SDK" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["february"] = new[] { "Legacy DB Migration" },
                    ["april"] = new[] { "Compliance Audit" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["march"] = new[] { "Vendor SDK Delay" },
                    ["april"] = new[] { "Security Review Pending", "Capacity Planning" }
                }
            }
        };

        var path = WriteJson(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);

        // Header data
        Assert.Equal("Project Phoenix - Executive Dashboard", svc.Data!.Title);
        Assert.Contains("Cloud Platform Team", svc.Data.Subtitle);
        Assert.Contains("dev.azure.com", svc.Data.BacklogLink);
        Assert.Equal("April", svc.Data.CurrentMonth);
        Assert.Equal(6, svc.Data.Months.Count);

        // Timeline data
        Assert.Equal(3, svc.Data.Timeline.Tracks.Count);
        Assert.Equal(3, svc.Data.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal(2, svc.Data.Timeline.Tracks[1].Milestones.Count);
        Assert.Equal(3, svc.Data.Timeline.Tracks[2].Milestones.Count);

        // Heatmap data
        Assert.Equal(4, svc.Data.Heatmap.Shipped.Count);
        Assert.Equal(2, svc.Data.Heatmap.InProgress.Count);
        Assert.Equal(2, svc.Data.Heatmap.Carryover.Count);
        Assert.Equal(2, svc.Data.Heatmap.Blockers.Count);
    }

    #endregion

    #region Large Data Scenarios

    [Fact]
    public async Task LoadAsync_ManyTracks_HandlesCorrectly()
    {
        var tracks = Enumerable.Range(1, 10).Select(i => new
        {
            name = $"M{i}",
            label = $"Track {i} - {new string('X', 30)}",
            color = $"#{i:X2}{i:X2}F4",
            milestones = Enumerable.Range(1, 5).Select(j => new
            {
                date = $"2026-{j:D2}-15",
                type = j % 3 == 0 ? "checkpoint" : j % 2 == 0 ? "production" : "poc",
                label = $"Milestone {j} of Track {i}"
            }).ToArray()
        }).ToArray();

        var data = new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };

        var path = WriteJson(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(10, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal(5, svc.Data.Timeline.Tracks[0].Milestones.Count);
        Assert.Equal(5, svc.Data.Timeline.Tracks[9].Milestones.Count);
    }

    [Fact]
    public async Task LoadAsync_ManyHeatmapItems_HandlesCorrectly()
    {
        var months = new[] { "jan", "feb", "mar", "apr", "may", "jun" };
        var shipped = months.ToDictionary(
            m => m,
            m => Enumerable.Range(1, 10).Select(i => $"Shipped {m} Item {i}").ToArray()
        );

        var data = new
        {
            title = "T",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped,
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };

        var path = WriteJson(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(6, svc.Data!.Heatmap.Shipped.Count);
        foreach (var month in months)
        {
            Assert.Equal(10, svc.Data.Heatmap.Shipped[month].Count);
        }
    }

    #endregion

    #region State Transitions With Full Data

    [Fact]
    public async Task LoadAsync_ValidThenCorrupt_TransitionsToErrorCleanly()
    {
        var validData = new
        {
            title = "Valid",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["apr"] = new[] { "Item A" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };

        var path = WriteJson(validData);
        var svc = CreateService();

        // First load: valid
        await svc.LoadAsync(path);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Valid", svc.Data!.Title);

        // Corrupt the file
        File.WriteAllText(path, "{{{{not json}}}");
        await svc.LoadAsync(path);

        // Should be in error state, data cleared
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
        Assert.NotNull(svc.ErrorMessage);
    }

    [Fact]
    public async Task LoadAsync_CorruptThenValid_RecoversFully()
    {
        var svc = CreateService();

        // First load: corrupt
        var corruptPath = Path.Combine(_tempDir, "corrupt.json");
        File.WriteAllText(corruptPath, "{ broken }}}");
        await svc.LoadAsync(corruptPath);
        Assert.True(svc.IsError);

        // Second load: valid
        var validData = new
        {
            title = "Recovered",
            subtitle = "S",
            backlogLink = "https://link",
            currentMonth = "April",
            months = new[] { "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };
        var validPath = WriteJson(validData);
        await svc.LoadAsync(validPath);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
        Assert.Equal("Recovered", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_MultipleConsecutiveValidLoads_LastDataWins()
    {
        var svc = CreateService();

        for (int i = 1; i <= 5; i++)
        {
            var data = new
            {
                title = $"Version {i}",
                subtitle = "S",
                backlogLink = "https://link",
                currentMonth = "April",
                months = new[] { "April" },
                timeline = new
                {
                    startDate = "2026-01-01",
                    endDate = "2026-07-01",
                    nowDate = "2026-04-10",
                    tracks = new[] { new { name = "M1", label = "L", color = "#000", milestones = Array.Empty<object>() } }
                },
                heatmap = new
                {
                    shipped = new Dictionary<string, string[]>(),
                    inProgress = new Dictionary<string, string[]>(),
                    carryover = new Dictionary<string, string[]>(),
                    blockers = new Dictionary<string, string[]>()
                }
            };
            var path = WriteJson(data);
            await svc.LoadAsync(path);
        }

        Assert.False(svc.IsError);
        Assert.Equal("Version 5", svc.Data!.Title);
    }

    #endregion

    #region JSON Format Variations

    [Fact]
    public async Task LoadAsync_MinifiedJson_ParsesCorrectly()
    {
        var json = """{"title":"T","subtitle":"S","backlogLink":"https://link","currentMonth":"April","months":["April"],"timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","nowDate":"2026-04-10","tracks":[{"name":"M1","label":"L","color":"#000","milestones":[]}]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";

        var path = Path.Combine(_tempDir, "minified.json");
        File.WriteAllText(path, json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("T", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_JsonWithExtraFields_IgnoresUnknown()
    {
        var json = """
        {
            "title": "T", "subtitle": "S", "backlogLink": "https://link",
            "currentMonth": "April", "months": ["April"],
            "unknownField": "should be ignored",
            "anotherUnknown": 42,
            "timeline": {
                "startDate": "2026-01-01", "endDate": "2026-07-01",
                "nowDate": "2026-04-10", "extraTimelineField": true,
                "tracks": [{"name":"M1","label":"L","color":"#000","milestones":[],"trackExtra":"ignored"}]
            },
            "heatmap": {"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;

        var path = Path.Combine(_tempDir, "extra_fields.json");
        File.WriteAllText(path, json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("T", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_JsonWithUnicodeContent_ParsesCorrectly()
    {
        var data = new
        {
            title = "Tableau de bord exécutif",
            subtitle = "Équipe développement - Avril 2026",
            backlogLink = "https://link",
            currentMonth = "Avril",
            months = new[] { "Avril" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "Plateforme 基盤", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["avril"] = new[] { "Module d'authentification" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };

        var path = WriteJson(data);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("Tableau de bord exécutif", svc.Data!.Title);
        Assert.Contains("Équipe développement", svc.Data.Subtitle);
        Assert.Contains("Module d'authentification", svc.Data.Heatmap.Shipped["avril"]);
    }

    #endregion
}