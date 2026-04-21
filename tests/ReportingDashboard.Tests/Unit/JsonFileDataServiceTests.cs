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

    private IOptions<DashboardOptions> CreateOptions(string fileName)
    {
        return Options.Create(new DashboardOptions
        {
            DataFilePath = Path.Combine(_tempDir, fileName)
        });
    }

    [Fact]
    public void GetData_ValidJsonFile_DeserializesCorrectly()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogUrl = "https://example.com",
            CurrentDate = "2026-04-01"
        };
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(Path.Combine(_tempDir, "valid.json"), json);

        var service = new JsonFileDataService(CreateOptions("valid.json"));

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("Test Project");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void GetData_MissingFile_ReturnsErrorWithPath()
    {
        var service = new JsonFileDataService(CreateOptions("nonexistent.json"));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("file not found");
    }

    [Fact]
    public void GetData_MalformedJson_ReturnsParseError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "bad.json"), "{ not valid json!!!");

        var service = new JsonFileDataService(CreateOptions("bad.json"));

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNull();
        service.GetError().Should().Contain("Could not parse");
    }

    [Fact]
    public void GetData_EmptyJsonObject_DeserializesWithDefaults()
    {
        File.WriteAllText(Path.Combine(_tempDir, "empty.json"), "{}");

        var service = new JsonFileDataService(CreateOptions("empty.json"));

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().BeEmpty();
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void GetData_NullDataFilePath_UsesDefault()
    {
        var options = Options.Create(new DashboardOptions { DataFilePath = null! });

        var service = new JsonFileDataService(options);

        // Default path ./data.json likely doesn't exist, so we expect an error
        service.GetError().Should().NotBeNull();
    }
}