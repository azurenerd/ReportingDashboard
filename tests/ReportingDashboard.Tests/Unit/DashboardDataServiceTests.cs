using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_test_{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "data");
        Directory.CreateDirectory(_dataDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public async Task GetDashboardDataAsync_ValidJson_ReturnsData()
    {
        var json = """
        {
            "title": "Test Dashboard",
            "subtitle": "Test Sub",
            "timelineMonths": ["Jan"],
            "currentMonth": "Jan",
            "milestones": [],
            "heatmap": { "columns": [], "rows": [] }
        }
        """;
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "data.json"), json);

        var service = new DashboardDataService(_envMock.Object);
        var result = await service.GetDashboardDataAsync();

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Dashboard");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardDataAsync_MissingFile_ReturnsNullWithFileNotFoundError()
    {
        // Don't create data.json
        var service = new DashboardDataService(_envMock.Object);
        var result = await service.GetDashboardDataAsync();

        result.Should().BeNull();
        service.GetError().Should().Contain("Unable to load dashboard data");
        service.GetError().Should().Contain("File not found");
    }

    [Fact]
    public async Task GetDashboardDataAsync_MalformedJson_ReturnsNullWithParseError()
    {
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "data.json"), "{invalid json content");

        var service = new DashboardDataService(_envMock.Object);
        var result = await service.GetDashboardDataAsync();

        result.Should().BeNull();
        service.GetError().Should().Contain("Unable to load dashboard data");
        service.GetError().Should().Contain("JSON parse error");
    }

    [Fact]
    public async Task GetDashboardDataAsync_EmptyJsonObject_AppliesDefaults()
    {
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "data.json"), "{}");

        var service = new DashboardDataService(_envMock.Object);
        var result = await service.GetDashboardDataAsync();

        result.Should().NotBeNull();
        result!.Title.Should().Be("");
        result.Subtitle.Should().Be("");
        result.TimelineMonths.Should().BeEmpty();
        result.CurrentMonth.Should().Be("");
        result.Milestones.Should().BeEmpty();
        result.Heatmap.Should().NotBeNull();
        result.Heatmap!.Columns.Should().BeEmpty();
        result.Heatmap.Rows.Should().BeEmpty();
        service.GetError().Should().BeNull();
    }

    [Fact]
    public async Task InvalidateCache_ForcesReloadOnNextCall()
    {
        await File.WriteAllTextAsync(Path.Combine(_dataDir, "data.json"),
            """{"title": "Original"}""");

        var service = new DashboardDataService(_envMock.Object);
        var result1 = await service.GetDashboardDataAsync();
        result1!.Title.Should().Be("Original");

        await File.WriteAllTextAsync(Path.Combine(_dataDir, "data.json"),
            """{"title": "Updated"}""");
        service.InvalidateCache();

        var result2 = await service.GetDashboardDataAsync();
        result2!.Title.Should().Be("Updated");
    }
}