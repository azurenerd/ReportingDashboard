using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class DashboardDataServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly string _tempDir;
    private readonly string _dataDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "data");
        Directory.CreateDirectory(_dataDir);

        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService() => new(_mockEnv.Object);

    private void WriteDataJson(string json)
    {
        File.WriteAllText(Path.Combine(_dataDir, "data.json"), json);
    }

    private static string ValidJson => """
        {
            "title": "Project Phoenix",
            "subtitle": "Cloud Engineering",
            "backlogLink": "https://dev.azure.com/contoso",
            "currentMonth": "Apr",
            "months": ["Jan","Feb","Mar","Apr","May","Jun"],
            "milestones": [],
            "heatmapRows": []
        }
        """;

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_ValidJson_ReturnsConfig()
    {
        WriteDataJson(ValidJson);
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().NotBeNull();
        result!.Title.Should().Be("Project Phoenix");
        result.Months.Should().HaveCount(6);
        service.LoadError.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_CalledTwice_ReturnsCachedInstance()
    {
        WriteDataJson(ValidJson);
        var service = CreateService();

        var first = await service.GetDashboardConfigAsync();
        var second = await service.GetDashboardConfigAsync();

        second.Should().BeSameAs(first, "second call should return the cached reference");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_MissingFile_ReturnsNullWithLoadError()
    {
        // Do not write any file — data.json does not exist
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().BeNull();
        service.LoadError.Should().NotBeNull();
        service.LoadError.Should().Contain("Failed to load data.json");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_MalformedJson_ReturnsNullWithParsingError()
    {
        WriteDataJson("{ invalid json !!!");
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().BeNull();
        service.LoadError.Should().NotBeNull();
        service.LoadError.Should().Contain("Failed to load data.json");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_MissingTitle_ReturnsNullWithValidationError()
    {
        var json = """
            {
                "title": "",
                "months": ["Jan","Feb"]
            }
            """;
        WriteDataJson(json);
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().BeNull();
        service.LoadError.Should().Contain("Required field 'title'");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_EmptyMonths_ReturnsNullWithValidationError()
    {
        var json = """
            {
                "title": "Valid Title",
                "months": []
            }
            """;
        WriteDataJson(json);
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().BeNull();
        service.LoadError.Should().Contain("Required field 'months'");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_EmptyOptionalArrays_LoadsSuccessfully()
    {
        WriteDataJson(ValidJson); // milestones: [], heatmapRows: [] are present but empty
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().NotBeNull();
        result!.Milestones.Should().NotBeNull().And.BeEmpty();
        result.HeatmapRows.Should().NotBeNull().And.BeEmpty();
        service.LoadError.Should().BeNull();
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_CaseInsensitiveDeserialization_Works()
    {
        var json = """
            {
                "Title": "Phoenix Upper",
                "Months": ["Jan","Feb","Mar"]
            }
            """;
        WriteDataJson(json);
        var service = CreateService();

        var result = await service.GetDashboardConfigAsync();

        result.Should().NotBeNull();
        result!.Title.Should().Be("Phoenix Upper");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public async Task GetDashboardConfigAsync_FailureNotCached_AllowsRetry()
    {
        // First call: file missing → failure
        var service = CreateService();
        var first = await service.GetDashboardConfigAsync();
        first.Should().BeNull();
        service.LoadError.Should().NotBeNull();

        // Now create the file and retry
        WriteDataJson(ValidJson);
        var second = await service.GetDashboardConfigAsync();

        second.Should().NotBeNull("failure should not be cached; retry should succeed");
        service.LoadError.Should().BeNull();
    }
}