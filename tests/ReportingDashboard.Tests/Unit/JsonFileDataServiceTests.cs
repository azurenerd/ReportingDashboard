using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private JsonFileDataService CreateService(string filePath)
    {
        var options = Options.Create(new DashboardOptions { DataFilePath = filePath });
        return new JsonFileDataService(options);
    }

    [Fact]
    public void ValidJsonFile_ReturnsData()
    {
        var dataPath = Path.Combine(_tempDir, "data.json");
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-10",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-07-01",
                Tracks = new List<TimelineTrack>
                {
                    new() { Id = "M1", Label = "Track 1", Color = "#0078D4", Milestones = new List<Milestone>() }
                }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "Jan" },
                CurrentMonth = "Jan",
                Categories = new List<HeatmapCategory>
                {
                    new() { Name = "Shipped", ColorClass = "ship", Items = new Dictionary<string, List<string>> { ["Jan"] = new() { "Item1" } } }
                }
            }
        };

        File.WriteAllText(dataPath, JsonSerializer.Serialize(data));

        var service = CreateService(dataPath);

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void MissingFile_ReturnsNullAndError()
    {
        var service = CreateService(Path.Combine(_tempDir, "nonexistent.json"));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
    }

    [Fact]
    public void MalformedJson_ReturnsNullAndError()
    {
        var dataPath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(dataPath, "{ invalid json!!!");

        var service = CreateService(dataPath);

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("Could not parse");
    }

    [Fact]
    public void EmptyJsonObject_ReturnsDataWithDefaults()
    {
        var dataPath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(dataPath, "{}");

        var service = CreateService(dataPath);

        service.GetData().Should().NotBeNull();
        service.GetError().Should().BeNull();
        service.GetData()!.Title.Should().BeEmpty();
    }
}