using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// End-to-end integration tests for DashboardDataService focused on 
/// file system interactions, concurrent reload scenarios, and large file handling
/// not covered by existing DashboardDataServiceIntegrationTests.
/// </summary>
[Trait("Category", "Integration")]
public class DataServiceFileIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DataServiceFileIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DataSvcFile_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteFile(string content, string name = "data.json")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    #region Full Pipeline with Real File I/O

    [Fact]
    public async Task LoadAsync_RealFileWithBOM_DeserializesCorrectly()
    {
        var json = TestDataHelper.CreateValidDataJsonString();
        var path = Path.Combine(_tempDir, "bom.json");
        // Write with BOM (UTF-8 BOM)
        await File.WriteAllTextAsync(path, json, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Integration Test Dashboard", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_LargeDataFile_HandlesCorrectly()
    {
        // Create a large data file with many heatmap items
        var shipped = new Dictionary<string, List<string>>();
        for (int month = 1; month <= 12; month++)
        {
            var key = new DateTime(2026, month, 1).ToString("MMM").ToLower();
            shipped[key] = Enumerable.Range(1, 50).Select(i => $"Feature {month}-{i}").ToList();
        }

        var data = new
        {
            title = "Large Dashboard",
            subtitle = "Stress Test",
            backlogLink = "",
            currentMonth = "April",
            months = Enumerable.Range(1, 12).Select(m => new DateTime(2026, m, 1).ToString("MMMM")).ToArray(),
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-12-31",
                nowDate = "2026-04-10",
                tracks = Enumerable.Range(1, 10).Select(i => new
                {
                    name = $"M{i}",
                    label = $"Track {i}",
                    color = $"#{i:X2}{i:X2}{i:X2}",
                    milestones = Enumerable.Range(1, 5).Select(j => new
                    {
                        date = $"2026-{(j * 2):D2}-15",
                        type = j % 2 == 0 ? "poc" : "production",
                        label = $"M{i} milestone {j}"
                    }).ToArray()
                }).ToArray()
            },
            heatmap = new
            {
                shipped,
                inProgress = new Dictionary<string, List<string>> { ["apr"] = new() { "Big Feature" } },
                carryover = new Dictionary<string, List<string>>(),
                blockers = new Dictionary<string, List<string>>()
            }
        };

        var json = TestDataHelper.SerializeToJson(data);
        var path = WriteFile(json, "large.json");

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal(10, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal(12, svc.Data.Heatmap.Shipped.Count);
        Assert.Equal(50, svc.Data.Heatmap.Shipped["jan"].Count);
    }

    [Fact]
    public async Task LoadAsync_FileWithTrailingWhitespace_Succeeds()
    {
        var json = TestDataHelper.CreateMinimalValidJsonString() + "\n\n\n   ";
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_FileChangedBetweenLoads_ReflectsLatestContent()
    {
        var path = WriteFile(TestDataHelper.CreateValidDataJsonString());
        var svc = new DashboardDataService(_logger);

        await svc.LoadAsync(path);
        Assert.Equal("Integration Test Dashboard", svc.Data!.Title);

        // Update the file
        var updatedJson = TestDataHelper.SerializeToJson(new
        {
            title = "Updated Dashboard",
            subtitle = "V2",
            backlogLink = "",
            currentMonth = "May",
            months = new[] { "May" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-05-10",
                tracks = Array.Empty<object>()
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        });
        File.WriteAllText(path, updatedJson);

        await svc.LoadAsync(path);
        Assert.Equal("Updated Dashboard", svc.Data!.Title);
        Assert.Equal("May", svc.Data.CurrentMonth);
    }

    #endregion

    #region Concurrent Access

    [Fact]
    public async Task LoadAsync_ConcurrentLoads_DoNotThrow()
    {
        var path = WriteFile(TestDataHelper.CreateValidDataJsonString());

        var tasks = Enumerable.Range(0, 5).Select(_ =>
        {
            var svc = new DashboardDataService(_logger);
            return svc.LoadAsync(path);
        });

        var exceptions = await Record.ExceptionAsync(async () => await Task.WhenAll(tasks));
        Assert.Null(exceptions);
    }

    #endregion

    #region JSON Edge Cases via File I/O

    [Fact]
    public async Task LoadAsync_EmptyArraysInJson_DoesNotCrash()
    {
        var json = TestDataHelper.SerializeToJson(new
        {
            title = "Empty Arrays",
            subtitle = "",
            backlogLink = "",
            currentMonth = "",
            months = Array.Empty<string>(),
            timeline = new
            {
                startDate = "",
                endDate = "",
                nowDate = "",
                tracks = Array.Empty<object>()
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        });
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        // Should either succeed or set a validation error, but never throw
        if (!svc.IsError)
        {
            Assert.NotNull(svc.Data);
            Assert.Empty(svc.Data!.Months);
            Assert.Empty(svc.Data.Timeline.Tracks);
        }
    }

    [Fact]
    public async Task LoadAsync_MixedEmptyAndPopulatedHeatmap_LoadsCorrectly()
    {
        var json = TestDataHelper.SerializeToJson(new
        {
            title = "Mixed Heatmap",
            subtitle = "Test",
            backlogLink = "",
            currentMonth = "March",
            months = new[] { "January", "February", "March" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-03-15",
                tracks = new[]
                {
                    new
                    {
                        name = "M1",
                        label = "Track",
                        color = "#000",
                        milestones = Array.Empty<object>()
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Item1", "Item2", "Item3" }
                },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>
                {
                    ["feb"] = new[] { "Carried Item" }
                },
                blockers = new Dictionary<string, string[]>()
            }
        });
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal(3, svc.Data!.Heatmap.Shipped["jan"].Count);
        Assert.Empty(svc.Data.Heatmap.InProgress);
        Assert.Single(svc.Data.Heatmap.Carryover["feb"]);
        Assert.Empty(svc.Data.Heatmap.Blockers);
    }

    [Fact]
    public async Task LoadAsync_SpecialCharactersInData_PreservedCorrectly()
    {
        var json = TestDataHelper.SerializeToJson(new
        {
            title = "Dashboard with Special Chars: <>&\"'©®™",
            subtitle = "Team – Q1/Q2 • Deliverables — 2026",
            backlogLink = "https://dev.azure.com/org/project?query=status%3Dactive&sort=priority",
            currentMonth = "März",
            months = new[] { "Januar", "Februar", "März" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-03-15",
                tracks = Array.Empty<object>()
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Feature: Über-search™ (v2.0)" }
                },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        });
        var path = WriteFile(json);

        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Contains("©®™", svc.Data!.Title);
        Assert.Contains("—", svc.Data.Subtitle);
        Assert.Contains("query=status%3Dactive", svc.Data.BacklogLink);
        Assert.Contains("Über-search™", svc.Data.Heatmap.Shipped["jan"][0]);
    }

    #endregion

    #region Error Recovery Integration

    [Fact]
    public async Task LoadAsync_ErrorThenValidThenError_StatesTransitionCorrectly()
    {
        var svc = new DashboardDataService(_logger);

        // Step 1: Error (missing file)
        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));
        Assert.True(svc.IsError);
        Assert.Null(svc.Data);
        Assert.Contains("not found", svc.ErrorMessage!);

        // Step 2: Valid data
        var validPath = WriteFile(TestDataHelper.CreateValidDataJsonString());
        await svc.LoadAsync(validPath);
        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Null(svc.ErrorMessage);

        // Step 3: Malformed JSON
        var brokenPath = WriteFile("{{broken}", "broken.json");
        await svc.LoadAsync(brokenPath);
        Assert.True(svc.IsError);
        Assert.Contains("parse", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}