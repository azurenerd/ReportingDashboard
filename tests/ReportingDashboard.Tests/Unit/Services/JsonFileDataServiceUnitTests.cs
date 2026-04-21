using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

public class JsonFileDataServiceUnitTests : IDisposable
{
    private readonly string _tempDir;

    public JsonFileDataServiceUnitTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
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
    public void ValidJson_ReturnsPopulatedData_AndNullError()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-15",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                Tracks = new List<TimelineTrack>
                {
                    new() { Id = "T1", Label = "Track One", Color = "#0078D4", Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-01", Type = "poc", Label = "PoC Done" }
                    }}
                }
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "Jan", "Feb" },
                CurrentMonth = "Feb",
                Categories = new List<HeatmapCategory>
                {
                    new() { Name = "Shipped", ColorClass = "ship", Items = new Dictionary<string, List<string>>
                    {
                        ["Jan"] = new() { "Item A" }
                    }}
                }
            }
        };

        var filePath = Path.Combine(_tempDir, "valid.json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(data));

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetError().Should().BeNull();
        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetData()!.Timeline.Tracks.Should().HaveCount(1);
        service.GetData()!.Heatmap.Categories.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MissingFile_ReturnsNullData_AndErrorMessage()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.json");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MalformedJson_ReturnsNullData_AndParseError()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{ this is not valid json!!! }");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("Could not parse data.json");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EmptyJsonObject_ReturnsDataWithDefaults()
    {
        var filePath = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(filePath, "{}");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetError().Should().BeNull();
        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().BeEmpty();
        service.GetData()!.Timeline.Tracks.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CaseInsensitiveDeserialization_Works()
    {
        var filePath = Path.Combine(_tempDir, "case.json");
        File.WriteAllText(filePath, """{"Title":"CaseTest","subtitle":"sub","BACKLOGURL":"http://x"}""");

        var service = new JsonFileDataService(CreateOptions(filePath));

        service.GetError().Should().BeNull();
        service.GetData()!.Title.Should().Be("CaseTest");
        service.GetData()!.Subtitle.Should().Be("sub");
        service.GetData()!.BacklogUrl.Should().Be("http://x");
    }
}