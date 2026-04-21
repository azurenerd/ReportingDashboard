using System.Text.Json;
using Microsoft.Extensions.Options;
using Xunit;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

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

    private IOptions<DashboardOptions> CreateOptions(string filePath) =>
        Options.Create(new DashboardOptions { DataFilePath = filePath });

    [Fact]
    public void GetData_ValidFile_ReturnsDeserializedData()
    {
        var filePath = Path.Combine(_tempDir, "data.json");
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
                Tracks = new List<TimelineTrack>
                {
                    new() { Id = "M1", Label = "Track 1", Color = "#0078D4" }
                }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "March", "April" },
                CurrentMonth = "April",
                Categories = new List<HeatmapCategory>
                {
                    new()
                    {
                        Name = "Shipped",
                        ColorClass = "ship",
                        Items = new Dictionary<string, List<string>>
                        {
                            ["March"] = new() { "Item 1" },
                            ["April"] = new() { "Item 2", "Item 3" }
                        }
                    }
                }
            }
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllText(filePath, json);

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetError());
        Assert.NotNull(service.GetData());
        Assert.Equal("Test Project", service.GetData()!.Title);
        Assert.Single(service.GetData()!.Timeline.Tracks);
        Assert.Equal("April", service.GetData()!.Heatmap.CurrentMonth);
    }

    [Fact]
    public void GetData_MissingFile_NoSampleFallback_ReturnsError()
    {
        // Point to a directory with no data.json AND no data.sample.json
        var isolatedDir = Path.Combine(_tempDir, "empty_subdir");
        Directory.CreateDirectory(isolatedDir);
        var filePath = Path.Combine(isolatedDir, "data.json");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("file not found", service.GetError()!);
        Assert.Contains("data.sample.json", service.GetError()!);
    }

    [Fact]
    public void GetData_MissingFile_WithSampleFallback_ReturnsSampleData()
    {
        // Create data.sample.json but not data.json
        var samplePath = Path.Combine(_tempDir, "data.sample.json");
        var sampleData = new DashboardData { Title = "Sample Fallback" };
        File.WriteAllText(samplePath, JsonSerializer.Serialize(sampleData));

        var filePath = Path.Combine(_tempDir, "data.json");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetError());
        Assert.NotNull(service.GetData());
        Assert.Equal("Sample Fallback", service.GetData()!.Title);
    }

    [Fact]
    public void GetData_MalformedJson_ReturnsError()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{ this is not valid json }}}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetData());
        Assert.NotNull(service.GetError());
        Assert.Contains("Could not parse data.json", service.GetError()!);
    }

    [Fact]
    public void GetData_EmptyJsonObject_ReturnsDataWithDefaults()
    {
        var filePath = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(filePath, "{}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetError());
        Assert.NotNull(service.GetData());
        Assert.Equal("", service.GetData()!.Title);
    }

    [Fact]
    public void GetData_CaseInsensitiveDeserialization_Works()
    {
        var filePath = Path.Combine(_tempDir, "case.json");
        File.WriteAllText(filePath, """{"Title":"Upper Case","SUBTITLE":"ALL CAPS"}""");

        var service = new JsonFileDataService(CreateOptions(filePath));

        Assert.Null(service.GetError());
        Assert.NotNull(service.GetData());
        Assert.Equal("Upper Case", service.GetData()!.Title);
    }
}