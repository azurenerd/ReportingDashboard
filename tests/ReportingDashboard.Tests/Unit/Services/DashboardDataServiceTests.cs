using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

internal class TestWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = "";
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "ReportingDashboard";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = "";
    public string EnvironmentName { get; set; } = "Development";
}

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataFilePath;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashDataSvcTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _dataFilePath = Path.Combine("data", "data.json");
        Directory.CreateDirectory(Path.Combine(_tempDir, "data"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService(string? dataFilePath = null)
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        return new DashboardDataService(logger);
    }

    private string GetFullDataPath()
    {
        return Path.Combine(_tempDir, _dataFilePath);
    }

    private void WriteDataJson(object data)
    {
        var fullPath = GetFullDataPath();
        var dir = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        File.WriteAllText(fullPath, json);
    }

    private void WriteRawJson(string json)
    {
        var fullPath = GetFullDataPath();
        File.WriteAllText(fullPath, json);
    }

    [Fact]
    public async Task LoadAsync_ValidJson_DeserializesAllFields()
    {
        WriteDataJson(new
        {
            title = "Test Project",
            subtitle = "Team X",
            months = new[] { "January" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-01",
                tracks = new[] { new { name = "M1", label = "Track", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });

        var svc = CreateService();
        await svc.LoadAsync(GetFullDataPath());

        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();
        svc.Data!.Title.Should().Be("Test Project");
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_SetsIsErrorAndErrorMessage()
    {
        var svc = CreateService();
        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent", "path.json"));

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().NotBeNullOrEmpty();
        svc.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_SetsErrorWithDetails()
    {
        WriteRawJson("{ invalid json content !!! }");

        var svc = CreateService();
        await svc.LoadAsync(GetFullDataPath());

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().NotBeNullOrEmpty();
        svc.ErrorMessage.Should().Contain("parse");
    }

    [Fact]
    public async Task LoadAsync_NullDeserialization_SetsError()
    {
        WriteRawJson("null");

        var svc = CreateService();
        await svc.LoadAsync(GetFullDataPath());

        svc.IsError.Should().BeTrue();
        svc.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoadAsync_EmptyObject_SetsValidationError()
    {
        WriteRawJson("{}");

        var svc = CreateService();
        await svc.LoadAsync(GetFullDataPath());

        svc.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ConsecutiveLoads_ReflectUpdatedFile()
    {
        WriteDataJson(new
        {
            title = "V1",
            subtitle = "Sub",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-01", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });
        var svc = CreateService();

        await svc.LoadAsync(GetFullDataPath());
        svc.Data!.Title.Should().Be("V1");

        WriteDataJson(new
        {
            title = "V2",
            subtitle = "Sub",
            months = new[] { "Jan" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-01", tracks = new[] { new { name = "M1", label = "T", color = "#000", milestones = Array.Empty<object>() } } },
            heatmap = new { shipped = new Dictionary<string, string[]>(), inProgress = new Dictionary<string, string[]>(), carryover = new Dictionary<string, string[]>(), blockers = new Dictionary<string, string[]>() }
        });

        await svc.LoadAsync(GetFullDataPath());
        svc.Data!.Title.Should().Be("V2");
    }
}