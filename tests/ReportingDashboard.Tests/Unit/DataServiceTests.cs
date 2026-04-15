using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DataServiceTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static string BuildValidJson() => JsonSerializer.Serialize(new
    {
        header = new
        {
            title = "Project Phoenix",
            subtitle = "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
            backlogUrl = "https://dev.azure.com/org/project/_backlogs",
            currentMonth = "April 2026"
        },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-12-31",
            nowDate = "2026-04-15",
            tracks = new[]
            {
                new { id = "m1", label = "M1", description = "Core API & Auth", color = "#0078D4" },
                new { id = "m2", label = "M2", description = "PDS & Data Inventory", color = "#00897B" },
                new { id = "m3", label = "M3", description = "Auto Review DFD", color = "#546E7A" }
            },
            milestones = new[]
            {
                new { trackId = "m1", date = "2026-03-26", label = "PoC", type = "poc", description = (string?)null },
                new { trackId = "m2", date = "2026-05-15", label = "Production Release", type = "production", description = "Full rollout" }
            }
        },
        heatmap = new
        {
            columns = new[] { "Jan", "Feb", "Mar", "Apr" },
            highlightColumnIndex = 3,
            rows = new[]
            {
                new
                {
                    category = "Shipped",
                    colorTheme = "green",
                    cells = new[]
                    {
                        new { items = new[] { "Foundation scaffolding" } },
                        new { items = new[] { "Auth module" } },
                        new { items = new[] { "API endpoints" } },
                        new { items = Array.Empty<string>() }
                    }
                },
                new
                {
                    category = "In Progress",
                    colorTheme = "blue",
                    cells = new[]
                    {
                        new { items = Array.Empty<string>() },
                        new { items = new[] { "Dashboard UI" } },
                        new { items = new[] { "Timeline component" } },
                        new { items = new[] { "Heatmap grid" } }
                    }
                }
            }
        }
    });

    /// <summary>
    /// Builds JSON using the WRONG flat schema (matching the buggy shipped data.json)
    /// to verify it does NOT deserialize into a valid DashboardData.
    /// </summary>
    private static string BuildIncorrectSchemaJson() => @"{
        ""title"": ""Executive Reporting Dashboard"",
        ""subtitle"": ""Project Status Overview"",
        ""backlogLink"": """",
        ""currentMonth"": ""January"",
        ""months"": [""January"", ""February""],
        ""milestones"": [{ ""title"": ""Phase 1"", ""targetDate"": ""2026-02-01"", ""status"": ""Completed"" }],
        ""shipped"": [{ ""title"": ""Foundation"", ""description"": ""Scaffolding"" }],
        ""timeline"": { ""startMonth"": ""Jan 2026"", ""endMonth"": ""Jun 2026"", ""tracks"": [] },
        ""heatmap"": { ""months"": [""Jan""], ""categories"": [], ""items"": [] }
    }";

    [Fact]
    public void ValidJson_LoadsDataSuccessfully()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        using var service = new DataService(_tempDir);

        service.GetData().Should().NotBeNull();
        service.GetData()!.Header.Title.Should().Be("Project Phoenix");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void ValidJson_DeserializesAllSubObjects()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        using var service = new DataService(_tempDir);
        var data = service.GetData();

        data.Should().NotBeNull();

        // Header uses backlogUrl (not backlogLink)
        data!.Header.Should().NotBeNull();
        data.Header.Title.Should().Be("Project Phoenix");
        data.Header.BacklogUrl.Should().Be("https://dev.azure.com/org/project/_backlogs");
        data.Header.CurrentMonth.Should().Be("April 2026");

        // Timeline has proper structure with tracks and milestones
        data.Timeline.Should().NotBeNull();
        data.Timeline.Tracks.Should().HaveCount(3);
        data.Timeline.Tracks[0].Id.Should().Be("m1");
        data.Timeline.Milestones.Should().HaveCount(2);
        data.Timeline.Milestones[0].TrackId.Should().Be("m1");
        data.Timeline.Milestones[0].Label.Should().Be("PoC");
        data.Timeline.Milestones[0].Type.Should().Be("poc");
        data.Timeline.StartDate.Should().Be(new DateTime(2026, 1, 1));
        data.Timeline.NowDate.Should().Be(new DateTime(2026, 4, 15));

        // Heatmap uses columns/highlightColumnIndex/rows (not months/categories/items)
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Columns.Should().BeEquivalentTo(new[] { "Jan", "Feb", "Mar", "Apr" });
        data.Heatmap.HighlightColumnIndex.Should().Be(3);
        data.Heatmap.Rows.Should().HaveCount(2);
        data.Heatmap.Rows[0].Category.Should().Be("Shipped");
        data.Heatmap.Rows[0].Cells.Should().HaveCount(4);
        data.Heatmap.Rows[0].Cells[0].Items.Should().Contain("Foundation scaffolding");
    }

    [Fact]
    public void IncorrectSchema_ProducesNullSubObjects()
    {
        // This test documents the bug: if data.json uses the wrong flat schema,
        // deserialization succeeds but produces null/default sub-objects
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildIncorrectSchemaJson());

        using var service = new DataService(_tempDir);
        var data = service.GetData();

        // The incorrect schema will either fail to deserialize (returning null with error)
        // or produce an object with null Header (since "header" key is missing)
        if (data is not null)
        {
            // If it somehow deserializes, Header should be null because
            // the flat schema has no "header" object
            data.Header.Should().BeNull(
                "a flat schema with 'title'/'backlogLink' at root level does not map to the nested 'header.title'/'header.backlogUrl' structure");
        }
        else
        {
            service.GetError().Should().NotBeNullOrEmpty(
                "deserialization of incompatible schema should produce an error");
        }
    }

    [Fact]
    public void MissingFile_ReturnsNullDataWithError()
    {
        using var service = new DataService(_tempDir);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("data.json not found");
        service.GetError().Should().Contain(_tempDir);
    }

    [Fact]
    public void MalformedJson_ReturnsNullDataWithParseError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{ invalid json !!! }");

        using var service = new DataService(_tempDir);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Failed to parse data.json");
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        var service = new DataService(_tempDir);
        var act = () => service.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void OnDataChanged_FiresAfterFileChange()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());
        using var service = new DataService(_tempDir);

        var eventFired = new ManualResetEventSlim(false);
        service.OnDataChanged += () => eventFired.Set();

        // Modify the file to trigger watcher
        Thread.Sleep(50);
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), BuildValidJson());

        var fired = eventFired.Wait(TimeSpan.FromSeconds(3));
        fired.Should().BeTrue("OnDataChanged should fire after file modification");
    }

    [Fact]
    public void DataJsonInWrongLocation_IsNotLoaded()
    {
        // data.json must only be in the content root, not in subdirectories
        var subDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "data.json"), BuildValidJson());
        // No data.json at _tempDir root

        using var service = new DataService(_tempDir);

        service.GetData().Should().BeNull("DataService should only read data.json from content root path");
        service.GetError().Should().Contain("data.json not found");
    }
}