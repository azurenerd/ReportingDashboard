using System.Text.Json;
using Microsoft.Extensions.Options;
using Xunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
    public void ValidFile_LoadsData()
    {
        var path = Path.Combine(_tempDir, "data.json");
        var json = JsonSerializer.Serialize(new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-10",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "March", "April" },
                CurrentMonth = "April",
                Categories = new List<HeatmapCategory>()
            }
        });
        File.WriteAllText(path, json);

        var service = new JsonFileDataService(CreateOptions(path));

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void MissingFile_ReturnsError()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");

        var service = new JsonFileDataService(CreateOptions(path));

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("file not found");
    }

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ not valid json }}}");

        var service = new JsonFileDataService(CreateOptions(path));

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Could not parse data.json");
    }

    [Fact]
    public void EmptyJson_ReturnsDataWithDefaults()
    {
        var path = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(path, "{}");

        var service = new JsonFileDataService(CreateOptions(path));

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("");
        service.GetError().Should().BeNull();
    }
}
