using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    [Fact]
    public void Initialize_WithValidJson_LoadsData()
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        var json = JsonSerializer.Serialize(new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Unit Test"
        });
        File.WriteAllText(filePath, json);

        var options = new DashboardDataServiceOptions { FilePath = filePath };
        using var service = new DashboardDataService(options);

        service.Initialize();

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("Test Dashboard");
        service.Data.Subtitle.Should().Be("Unit Test");
        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Initialize_WithMissingFile_SetsErrorMessage()
    {
        var filePath = Path.Combine(_tempDir, "nonexistent.json");
        var options = new DashboardDataServiceOptions { FilePath = filePath };
        using var service = new DashboardDataService(options);

        service.Initialize();

        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("Data file not found");
        service.ErrorMessage.Should().Contain(filePath);
    }

    [Fact]
    public void Initialize_WithInvalidJson_SetsErrorMessage()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "{ not valid json !!!");

        var options = new DashboardDataServiceOptions { FilePath = filePath };
        using var service = new DashboardDataService(options);

        service.Initialize();

        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("Invalid JSON in data file");
    }

    [Fact]
    public void Initialize_WithEmptyJsonObject_SetsDataWithDefaults()
    {
        var filePath = Path.Combine(_tempDir, "empty.json");
        File.WriteAllText(filePath, "{}");

        var options = new DashboardDataServiceOptions { FilePath = filePath };
        using var service = new DashboardDataService(options);

        service.Initialize();

        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("");
        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void DashboardDataServiceOptions_DefaultFilePath_IsDataJson()
    {
        var options = new DashboardDataServiceOptions();

        options.FilePath.Should().Be("data.json");
    }
}