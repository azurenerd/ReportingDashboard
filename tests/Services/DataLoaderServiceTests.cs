using System.Text.Json;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

/// <summary>
/// Unit tests for DataLoaderService.
/// Tests file I/O, JSON deserialization, error handling, and configuration resolution.
/// Coverage target: 80% (critical path for data loading and error scenarios)
/// </summary>
public class DataLoaderServiceTests : IDisposable
{
    private readonly string _tempDirectory;

    public DataLoaderServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"DataLoaderServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    /// <summary>
    /// Test: LoadAsync succeeds with valid data.json file
    /// Verifies: File is read, JSON is deserialized, ProjectReport is returned with correct data
    /// </summary>
    [Fact]
    public async Task LoadAsync_WithValidDataJson_ReturnsProjectReport()
    {
        // Arrange
        var validJson = """
        {
          "projectName": "Test Project",
          "reportingPeriod": "2026-Q1",
          "milestones": [
            {
              "id": "m1",
              "name": "Phase 1",
              "targetDate": "2026-04-01",
              "status": "on-track",
              "progress": 50
            }
          ],
          "statusSnapshot": {
            "shipped": ["Item 1"],
            "inProgress": ["Item 2"],
            "carriedOver": []
          },
          "kpis": {
            "onTimeDelivery": 85
          }
        }
        """;

        var filePath = Path.Combine(_tempDirectory, "data.json");
        await File.WriteAllTextAsync(filePath, validJson);

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AppSettings:DataPath"]).Returns("data.json");

        var mockLogger = new Mock<ILogger<DataLoaderService>>();

        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act
        var result = await service.LoadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Project", result.ProjectName);
        Assert.Equal("2026-Q1", result.ReportingPeriod);
        Assert.Single(result.Milestones);
        Assert.Equal("Phase 1", result.Milestones[0].Name);
        Assert.Equal(50, result.Milestones[0].Progress);
        Assert.Single(result.StatusSnapshot.Shipped);
        Assert.Equal("Item 1", result.StatusSnapshot.Shipped[0]);

        // Verify logging
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Loading data from")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully loaded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test: LoadAsync throws FileNotFoundException when file does not exist
    /// Verifies: Exception is thrown with appropriate message and ERROR log
    /// </summary>
    [Fact]
    public async Task LoadAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var missingFilePath = Path.Combine(_tempDirectory, "nonexistent.json");
        
        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<DataLoaderService>>();

        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.LoadAsync(missingFilePath));

        Assert.Contains("not found", exception.Message);

        // Verify error logging
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Data file not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test: LoadAsync throws JsonException when JSON is malformed
    /// Verifies: Deserialization error is caught and propagated with ERROR log
    /// </summary>
    [Fact]
    public async Task LoadAsync_MalformedJson_ThrowsJsonException()
    {
        // Arrange
        var malformedJson = "{ invalid json syntax }";
        var filePath = Path.Combine(_tempDirectory, "data.json");
        await File.WriteAllTextAsync(filePath, malformedJson);

        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<DataLoaderService>>();

        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<JsonException>(() => service.LoadAsync(filePath));

        // Verify error logging
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("JSON deserialization failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test: GetConfiguredDataPath returns configured value from appsettings
    /// Verifies: Configuration is read and INFO log is produced
    /// </summary>
    [Fact]
    public void GetConfiguredDataPath_ReturnsConfiguredValue()
    {
        // Arrange
        var configuredPath = "custom/path/data.json";
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AppSettings:DataPath"]).Returns(configuredPath);

        var mockLogger = new Mock<ILogger<DataLoaderService>>();
        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act
        var result = service.GetConfiguredDataPath();

        // Assert
        Assert.Equal(configuredPath, result);

        // Verify logging
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Configured data path")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test: GetConfiguredDataPath returns default when config is null or empty
    /// Verifies: Default "data.json" is returned when no config value set
    /// </summary>
    [Fact]
    public void GetConfiguredDataPath_ReturnsDefault_WhenConfigNull()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AppSettings:DataPath"]).Returns((string)null);

        var mockLogger = new Mock<ILogger<DataLoaderService>>();
        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act
        var result = service.GetConfiguredDataPath();

        // Assert
        Assert.Equal("data.json", result);
    }

    /// <summary>
    /// Test: LoadAsync respects explicit dataPath parameter over config
    /// Verifies: Parameter takes precedence in resolution order
    /// </summary>
    [Fact]
    public async Task LoadAsync_WithExplicitPath_UsesParameterOverConfig()
    {
        // Arrange
        var validJson = """
        {
          "projectName": "Explicit Path Project",
          "reportingPeriod": "2026-Q2",
          "milestones": [],
          "statusSnapshot": {
            "shipped": [],
            "inProgress": [],
            "carriedOver": []
          }
        }
        """;

        var filePath = Path.Combine(_tempDirectory, "explicit.json");
        await File.WriteAllTextAsync(filePath, validJson);

        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AppSettings:DataPath"]).Returns("some/other/path.json");

        var mockLogger = new Mock<ILogger<DataLoaderService>>();
        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act
        var result = await service.LoadAsync(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Explicit Path Project", result.ProjectName);
    }

    /// <summary>
    /// Test: LoadAsync throws IOException when file is locked
    /// Verifies: File access errors are properly caught and logged
    /// </summary>
    [Fact]
    public async Task LoadAsync_FileInUse_ThrowsIOException()
    {
        // Arrange
        var validJson = """
        {
          "projectName": "Test",
          "reportingPeriod": "2026-Q1",
          "milestones": [],
          "statusSnapshot": {
            "shipped": [],
            "inProgress": [],
            "carriedOver": []
          }
        }
        """;

        var filePath = Path.Combine(_tempDirectory, "locked.json");
        await File.WriteAllTextAsync(filePath, validJson);

        // Open file to lock it
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<DataLoaderService>>();
        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => service.LoadAsync(filePath));

        // Verify error logging
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("File access error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Test: LoadAsync logs milestone count on successful load
    /// Verifies: Success logging includes milestone count for observability
    /// </summary>
    [Fact]
    public async Task LoadAsync_LogsMilestoneCount_OnSuccess()
    {
        // Arrange
        var validJson = """
        {
          "projectName": "Multi-Milestone Project",
          "reportingPeriod": "2026-Q1",
          "milestones": [
            {
              "id": "m1",
              "name": "Phase 1",
              "targetDate": "2026-04-01",
              "status": "on-track",
              "progress": 50
            },
            {
              "id": "m2",
              "name": "Phase 2",
              "targetDate": "2026-05-01",
              "status": "at-risk",
              "progress": 25
            }
          ],
          "statusSnapshot": {
            "shipped": [],
            "inProgress": [],
            "carriedOver": []
          }
        }
        """;

        var filePath = Path.Combine(_tempDirectory, "data.json");
        await File.WriteAllTextAsync(filePath, validJson);

        var mockConfig = new Mock<IConfiguration>();
        var mockLogger = new Mock<ILogger<DataLoaderService>>();
        var service = new DataLoaderService(mockConfig.Object, mockLogger.Object);

        // Act
        await service.LoadAsync(filePath);

        // Assert - verify milestone count logged
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Loaded") && v.ToString().Contains("milestones")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}