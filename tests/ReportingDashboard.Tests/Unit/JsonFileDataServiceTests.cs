using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

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

    private IOptions<DashboardOptions> CreateOptions(string filePath)
    {
        var mock = new Mock<IOptions<DashboardOptions>>();
        mock.Setup(o => o.Value).Returns(new DashboardOptions { DataFilePath = filePath });
        return mock.Object;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidJsonFile_DeserializesCorrectly()
    {
        var path = Path.Combine(_tempDir, "data.json");
        var json = JsonSerializer.Serialize(new
        {
            title = "Test Project",
            subtitle = "Test Sub",
            backlogUrl = "https://example.com",
            currentDate = "2026-04-01",
            timeline = new { startDate = "2025-10-01", endDate = "2026-09-30", tracks = Array.Empty<object>() },
            heatmap = new { months = new[] { "Jan" }, currentMonth = "Jan", categories = Array.Empty<object>() }
        });
        File.WriteAllText(path, json);

        var svc = new JsonFileDataService(CreateOptions(path));

        svc.GetData().Should().NotBeNull();
        svc.GetData()!.Title.Should().Be("Test Project");
        svc.GetError().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MissingFile_ReturnsErrorWithPath()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");

        var svc = new JsonFileDataService(CreateOptions(path));

        svc.GetData().Should().BeNull();
        svc.GetError().Should().Contain("file not found");
        svc.GetError().Should().Contain(Path.GetFullPath(path));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void MalformedJson_ReturnsParseError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{ invalid json!!! }");

        var svc = new JsonFileDataService(CreateOptions(path));

        svc.GetData().Should().BeNull();
        svc.GetError().Should().Contain("Could not parse data.json");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void EmptyJsonObject_DeserializesWithDefaults()
    {
        var path = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(path, "{}");

        var svc = new JsonFileDataService(CreateOptions(path));

        svc.GetData().Should().NotBeNull();
        svc.GetData()!.Title.Should().BeEmpty();
        svc.GetError().Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NullDataFilePath_UsesDefaultPath()
    {
        var mock = new Mock<IOptions<DashboardOptions>>();
        mock.Setup(o => o.Value).Returns(new DashboardOptions { DataFilePath = null! });

        var svc = new JsonFileDataService(mock.Object);

        // Default path ./data.json likely doesn't exist in test context
        // Service should handle gracefully with an error, not throw
        (svc.GetError() != null || svc.GetData() != null).Should().BeTrue();
    }
}