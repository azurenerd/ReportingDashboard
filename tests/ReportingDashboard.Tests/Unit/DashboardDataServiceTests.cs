using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests
{
    [Fact]
    public async Task GetDataAsync_WithValidJsonFile_ReturnsDashboardReport()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "test-data.json");
        var validJson = """
            {
              "header": {"title": "Test", "subtitle": "Sub", "backlogLink": "#", "reportDate": "2026-04", "timelineStartDate": "2026-01-01", "timelineEndDate": "2026-04-30", "timelineMonths": ["Jan", "Feb", "Mar", "Apr"]},
              "timelineTracks": [],
              "heatmap": {"columns": [], "highlightColumnIndex": 0, "rows": []}
            }
            """;
        await File.WriteAllTextAsync(tempFile, validJson);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DashboardDataPath", tempFile }
            })
            .Build();
        var service = new DashboardDataService(config);

        // Act
        var result = await service.GetDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Header.Title.Should().Be("Test");
        result.Header.Subtitle.Should().Be("Sub");
        result.TimelineTracks.Should().BeEmpty();
        result.Heatmap.Columns.Should().BeEmpty();

        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDataAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DashboardDataPath", "/nonexistent/path/data.json" }
            })
            .Build();
        var service = new DashboardDataService(config);

        // Act & Assert
        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*Dashboard data file not found*");
    }

    [Fact]
    public async Task GetDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "invalid-data.json");
        await File.WriteAllTextAsync(tempFile, "{invalid json}");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DashboardDataPath", tempFile }
            })
            .Build();
        var service = new DashboardDataService(config);

        // Act & Assert
        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid JSON in data.json*");

        File.Delete(tempFile);
    }

    [Fact]
    public void Constructor_WithoutConfigDashboardDataPath_UsesDefaultPath()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var service = new DashboardDataService(config);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDataAsync_WithNullDeserialization_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "null-data.json");
        await File.WriteAllTextAsync(tempFile, "null");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DashboardDataPath", tempFile }
            })
            .Build();
        var service = new DashboardDataService(config);

        // Act & Assert
        var act = () => service.GetDataAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*data.json deserialized to null*");

        File.Delete(tempFile);
    }
}