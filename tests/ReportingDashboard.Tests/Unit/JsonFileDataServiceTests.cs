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

    private IOptions<DashboardOptions> CreateOptions(string filename = "data.json")
    {
        return Options.Create(new DashboardOptions
        {
            DataFilePath = Path.Combine(_tempDir, filename)
        });
    }

    [Fact]
    public void ValidJsonFile_ReturnsData()
    {
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
                Months = new List<string> { "Jan", "Feb" },
                CurrentMonth = "Feb",
                Categories = new List<HeatmapCategory>
                {
                    new() { Name = "Shipped", ColorClass = "ship", Items = new Dictionary<string, List<string>>() }
                }
            }
        };

        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = new JsonFileDataService(CreateOptions());

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void MissingFile_NoFallback_ReturnsError()
    {
        // Ensure neither data.json nor data.sample.json exist
        var service = new JsonFileDataService(CreateOptions());

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
    }

    [Fact]
    public void MalformedJson_ReturnsParseError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{ invalid json!!!");

        var service = new JsonFileDataService(CreateOptions());

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Could not parse");
    }

    [Fact]
    public void EmptyJsonObject_ReturnsDataWithDefaults()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{}");

        var service = new JsonFileDataService(CreateOptions());

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().BeEmpty();
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void MissingDataJson_FallsBackToSampleJson()
    {
        var sampleData = new DashboardData { Title = "Sample Fallback" };
        var json = JsonSerializer.Serialize(sampleData);
        File.WriteAllText(Path.Combine(_tempDir, "data.sample.json"), json);

        var service = new JsonFileDataService(CreateOptions());

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Sample Fallback");
        service.GetError().Should().BeNull();
    }
}