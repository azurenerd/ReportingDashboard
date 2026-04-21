using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
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

    private JsonFileDataService CreateService(string? dataPath = null)
    {
        var filePath = dataPath ?? Path.Combine(_tempDir, "data.json");
        var env = new FakeWebHostEnvironment { ContentRootPath = _tempDir };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DashboardOptions:DataFilePath"] = filePath
            })
            .Build();
        var logger = NullLogger<JsonFileDataService>.Instance;
        return new JsonFileDataService(env, config, logger);
    }

    [Fact]
    public async Task ValidJsonFile_ReturnsData()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            BacklogLink = "https://example.com",
            CurrentMonth = "Apr",
            Months = new List<string> { "Apr" },
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
                Shipped = new Dictionary<string, List<string>>
                {
                    ["Jan"] = new() { "Item1" }
                }
            }
        };

        var json = JsonSerializer.Serialize(data);
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "data.json"), json);

        var service = CreateService();
        var result = await service.LoadDashboardDataAsync();

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Project");
    }

    [Fact]
    public async Task MissingFile_ReturnsDefault()
    {
        var service = CreateService(Path.Combine(_tempDir, "nonexistent.json"));
        var result = await service.LoadDashboardDataAsync();

        result.Should().NotBeNull();
        result.Title.Should().Be("Dashboard");
    }

    [Fact]
    public async Task MalformedJson_ReturnsDefault()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "data.json"), "{ invalid json!!!");

        var service = CreateService();
        var result = await service.LoadDashboardDataAsync();

        result.Should().NotBeNull();
        result.Title.Should().Be("Dashboard");
    }

    [Fact]
    public async Task EmptyJsonObject_ReturnsDataWithDefaults()
    {
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "data.json"), "{}");

        var service = CreateService();
        var result = await service.LoadDashboardDataAsync();

        result.Should().NotBeNull();
        result.Title.Should().BeEmpty();
    }

    [Fact]
    public async Task CachesResult()
    {
        var data = new DashboardData { Title = "Cached" };
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "data.json"), JsonSerializer.Serialize(data));

        var service = CreateService();
        var first = await service.LoadDashboardDataAsync();
        var second = await service.LoadDashboardDataAsync();

        first.Should().BeSameAs(second);
    }

    private class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "Test";
        public string EnvironmentName { get; set; } = "Test";
    }
}