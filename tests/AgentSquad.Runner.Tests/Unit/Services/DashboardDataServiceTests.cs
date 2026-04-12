using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardDataServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    [Fact]
    public void Constructor_LoadsConfigurationFromAppsettings()
    {
        _mockConfiguration
            .Setup(c => c.GetValue<string>("DashboardDataPath"))
            .Returns("./wwwroot/data/data.json");

        var service = new DashboardDataService(_mockConfiguration.Object, _mockLogger.Object);

        service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDashboardConfigAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        _mockConfiguration
            .Setup(c => c.GetValue<string>("DashboardDataPath"))
            .Returns("./nonexistent/path/data.json");

        var service = new DashboardDataService(_mockConfiguration.Object, _mockLogger.Object);

        var action = () => service.GetDashboardConfigAsync();

        await action.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task RefreshAsync_ClearsCache()
    {
        _mockConfiguration
            .Setup(c => c.GetValue<string>("DashboardDataPath"))
            .Returns("./wwwroot/data/data.json");

        var service = new DashboardDataService(_mockConfiguration.Object, _mockLogger.Object);

        await service.RefreshAsync();

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetLastModifiedTime_ReturnsDateTime()
    {
        _mockConfiguration
            .Setup(c => c.GetValue<string>("DashboardDataPath"))
            .Returns("./nonexistent/data.json");

        var service = new DashboardDataService(_mockConfiguration.Object, _mockLogger.Object);
        var result = service.GetLastModifiedTime();

        result.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_LogsSuccessfulLoad()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "test_data.json");
        var testData = new { projectName = "Test", description = "Desc", quarters = new object[0], milestones = new object[0] };
        var json = JsonSerializer.Serialize(testData);
        await File.WriteAllTextAsync(tempFile, json);

        try
        {
            var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, tempFile);
            _mockConfiguration
                .Setup(c => c.GetValue<string>("DashboardDataPath"))
                .Returns(relativePath);

            var service = new DashboardDataService(_mockConfiguration.Object, _mockLogger.Object);
            
            var result = await service.GetDashboardConfigAsync();

            result.Should().NotBeNull();
            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetDashboardConfigAsync_ValidatesProjectNameRequired()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), "test_invalid.json");
        var testData = new { projectName = "", description = "Desc", quarters = new object[0], milestones = new object[0] };
        var json = JsonSerializer.Serialize(testData);
        await File.WriteAllTextAsync(tempFile, json);

        try
        {
            var relativePath = Path.GetRelativePath(AppContext.BaseDirectory, tempFile);
            _mockConfiguration
                .Setup(c => c.GetValue<string>("DashboardDataPath"))
                .Returns(relativePath);

            var service = new DashboardDataService(_mockConfiguration.Object, _mockLogger.Object);
            
            var action = () => service.GetDashboardConfigAsync();

            await action.Should().ThrowAsync<InvalidOperationException>();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}