using System.Text.Json;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ReportingDashboard.Tests.Services;

public class JsonFileDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataFilePath;

    public JsonFileDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "rdtest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _dataFilePath = Path.Combine(_tempDir, "data.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private JsonFileDataService CreateService(string? dataPath = null)
    {
        var env = new FakeWebHostEnvironment { ContentRootPath = _tempDir };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DashboardOptions:DataFilePath"] = dataPath ?? _dataFilePath
            })
            .Build();
        var logger = NullLogger<JsonFileDataService>.Instance;
        return new JsonFileDataService(env, config, logger);
    }

    [Fact]
    public async Task LoadDashboardDataAsync_ValidFile_ReturnsData()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test",
            CurrentMonth = "Apr",
            Months = new List<string> { "Apr" }
        };
        await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(data));

        var service = CreateService();
        var result = await service.LoadDashboardDataAsync();

        Assert.Equal("Test Project", result.Title);
    }

    [Fact]
    public async Task LoadDashboardDataAsync_MissingFile_ReturnsDefault()
    {
        var service = CreateService(Path.Combine(_tempDir, "nonexistent.json"));
        var result = await service.LoadDashboardDataAsync();

        Assert.Equal("Dashboard", result.Title);
        Assert.NotNull(result.Timeline);
        Assert.NotNull(result.Heatmap);
    }

    [Fact]
    public async Task LoadDashboardDataAsync_InvalidJson_ReturnsDefault()
    {
        await File.WriteAllTextAsync(_dataFilePath, "not valid json!!!");

        var service = CreateService();
        var result = await service.LoadDashboardDataAsync();

        Assert.Equal("Dashboard", result.Title);
    }

    [Fact]
    public async Task LoadDashboardDataAsync_CachesResult()
    {
        var data = new DashboardData { Title = "Cached" };
        await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(data));

        var service = CreateService();
        var first = await service.LoadDashboardDataAsync();
        var second = await service.LoadDashboardDataAsync();

        Assert.Same(first, second);
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