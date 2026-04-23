using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _tempFile;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dash_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "dashboard-data.json");
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private IDashboardDataService CreateService(string? filePath = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DashboardDataFile"] = filePath ?? _tempFile
            })
            .Build();

        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(_tempDir);

        return new DashboardDataService(config, env.Object);
    }

    private static string GetValidJson() => JsonSerializer.Serialize(new
    {
        project = new { title = "Test Project", subtitle = "Test Sub", backlogUrl = "https://example.com", currentDate = "2026-04-15" },
        timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = new[] { new { id = "M1", name = "Track 1", color = "#0078D4", milestones = Array.Empty<object>() } } },
        heatmap = new { months = new[] { "January", "February" }, highlightMonth = "February", rows = new[] { new { category = "Shipped", items = new Dictionary<string, string[]> { ["January"] = new[] { "Item A" } } } } }
    });

    [Fact]
    public void LoadsValidJson_ReturnsPopulatedData()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        using var service = CreateService();

        var data = service.GetData();
        var error = service.GetError();

        data.Should().NotBeNull();
        error.Should().BeNull();
        data!.Project.Title.Should().Be("Test Project");
        data.Project.Subtitle.Should().Be("Test Sub");
        data.Timeline.Tracks.Should().HaveCount(1);
        data.Heatmap.Months.Should().HaveCount(2);
    }

    [Fact]
    public void MissingFile_ReturnsDescriptiveError()
    {
        var missingPath = Path.Combine(_tempDir, "nonexistent.json");
        using var service = CreateService(missingPath);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("Dashboard data file not found");
        service.GetError().Should().Contain(missingPath);
    }

    [Fact]
    public void MalformedJson_ReturnsParseError()
    {
        File.WriteAllText(_tempFile, "{ invalid json !!!");
        using var service = CreateService();

        service.GetData().Should().BeNull();
        service.GetError().Should().StartWith("Error reading dashboard data:");
    }

    [Fact]
    public void NullBacklogUrl_DeserializesCorrectly()
    {
        var json = JsonSerializer.Serialize(new
        {
            project = new { title = "No Link", subtitle = "Sub", currentDate = "2026-04-15" },
            timeline = new { startDate = "2026-01-01", endDate = "2026-07-01", tracks = Array.Empty<object>() },
            heatmap = new { months = Array.Empty<string>(), highlightMonth = "", rows = Array.Empty<object>() }
        });
        File.WriteAllText(_tempFile, json);
        using var service = CreateService();

        var data = service.GetData();
        data.Should().NotBeNull();
        data!.Project.BacklogUrl.Should().BeNull();
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        File.WriteAllText(_tempFile, GetValidJson());
        var service = CreateService();
        var act = () => service.Dispose();
        act.Should().NotThrow();
    }
}