using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests.Unit;

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
    [Trait("Category", "Unit")]
    public void ValidJsonFile_DeserializesCorrectly()
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        var data = new
        {
            title = "Test Project",
            subtitle = "Test Subtitle",
            backlogUrl = "https://example.com",
            currentDate = "2026-04-01",
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                tracks = new[]
                {
                    new
                    {
                        id = "M1",
                        label = "Track 1",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { date = "2026-02-01", type = "checkpoint", label = "CP1" }
                        }
                    }
                }
            },
            heatmap = new
            {
                months = new[] { "Jan", "Feb" },
                currentMonth = "Feb",
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        colorClass = "ship",
                        items = new Dictionary<string, List<string>>
                        {
                            ["Jan"] = new List<string> { "Item 1" }
                        }
                    }
                }
            }
        };
        File.WriteAllText(filePath, JsonSerializer.Serialize(data));

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetError().Should().BeNull();
        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetData()!.Timeline.Tracks.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MissingFile_ReturnsError()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.json");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MalformedJson_ReturnsParseError()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{invalid json content!!");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("Could not parse data.json");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EmptyJsonObject_DeserializesWithDefaults()
    {
        var filePath = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(filePath, "{}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        // {} deserializes to a DashboardData with null/default properties
        // The service checks if _data is null after deserialization
        // An empty object {} may deserialize to a non-null object with default props
        service.GetError().Should().BeNull();
        service.GetData().Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NullDataFilePath_UsesDefaultPath()
    {
        var options = Options.Create(new DashboardOptions { DataFilePath = null! });

        // Should not throw; will use "./data.json" default which likely doesn't exist
        var service = new JsonFileDataService(options);

        // The default path "./data.json" likely doesn't exist in test context
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
    }
}