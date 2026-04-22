using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private IOptions<DashboardOptions> CreateOptions(string filePath)
    {
        return Options.Create(new DashboardOptions { DataFilePath = filePath });
    }

    [Fact]
    public void GetData_ValidFile_ReturnsDeserializedData()
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
                EndDate = "2026-06-30",
                Tracks = new List<TimelineTrack>
                {
                    new() { Id = "M1", Label = "Track 1", Color = "#0078D4", Milestones = new List<Milestone>() }
                }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "March", "April" },
                CurrentMonth = "April",
                Categories = new List<HeatmapCategory>()
            }
        };

        var filePath = Path.Combine(_tempDir, "valid.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().NotBeNull();
        service.GetError().Should().BeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetData()!.Timeline.Tracks.Should().HaveCount(1);
    }

    [Fact]
    public void GetData_MissingFile_ReturnsError()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.json");
        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("file not found");
        service.GetError().Should().Contain("data.sample.json");
    }

    [Fact]
    public void GetData_MalformedJson_ReturnsError()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{ invalid json content [[[");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Could not parse data.json");
    }

    [Fact]
    public void GetData_EmptyJsonObject_ReturnsDataWithDefaults()
    {
        var filePath = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(filePath, "{}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().NotBeNull();
        service.GetError().Should().BeNull();
        service.GetData()!.Title.Should().Be("");
        service.GetData()!.Timeline.Tracks.Should().BeEmpty();
    }

    [Fact]
    public void GetData_CaseInsensitiveProperties_Works()
    {
        var filePath = Path.Combine(_tempDir, "case.json");
        File.WriteAllText(filePath, "{\"Title\": \"Upper Case\", \"SUBTITLE\": \"should not map\"}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Upper Case");
    }
}
