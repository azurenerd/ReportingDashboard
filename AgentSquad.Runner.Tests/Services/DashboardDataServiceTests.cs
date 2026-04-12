using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class DashboardDataServiceTests : IAsyncLifetime
{
    private readonly string _testDirectory;
    private readonly string _dataFilePath;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    public DashboardDataServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _dataFilePath = Path.Combine(_testDirectory, "data.json");
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_testDirectory);
        _configurationMock
            .Setup(x => x.GetValue<string>("DashboardDataPath", It.IsAny<string>()))
            .Returns(_dataFilePath);
        _configurationMock
            .Setup(x => x.GetValue(typeof(string), "DashboardDataPath", It.IsAny<object>()))
            .Returns(_dataFilePath);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithValidJson_ReturnsDeserializedConfig()
    {
        var validJson = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": ["Item 1"],
              "inProgress": ["Item 2"],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Milestone 1",
              "date": "2026-03-15",
              "type": "checkpoint"
            }
          ]
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, validJson);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);
        var config = await service.GetDashboardConfigAsync();

        Assert.NotNull(config);
        Assert.Equal("Test Project", config.ProjectName);
        Assert.Equal("Test Description", config.Description);
        Assert.Single(config.Quarters);
        Assert.Single(config.Milestones);
        Assert.Equal("Item 1", config.Quarters[0].Shipped[0]);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithMissingFile_ThrowsFileNotFoundException()
    {
        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("data.json not found", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithInvalidJson_ThrowsJsonException()
    {
        var invalidJson = "{ invalid json }";
        await File.WriteAllTextAsync(_dataFilePath, invalidJson);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        await Assert.ThrowsAsync<JsonException>(
            () => service.GetDashboardConfigAsync());
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithMissingProjectName_ThrowsInvalidOperationException()
    {
        var jsonWithoutProjectName = """
        {
          "description": "Test Description",
          "quarters": [],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithoutProjectName);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("ProjectName", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithMissingDescription_ThrowsInvalidOperationException()
    {
        var jsonWithoutDescription = """
        {
          "projectName": "Test Project",
          "quarters": [],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithoutDescription);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("Description", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithEmptyQuarters_ThrowsInvalidOperationException()
    {
        var jsonWithoutQuarters = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithoutQuarters);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("Quarters", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithInvalidMonthName_ThrowsInvalidOperationException()
    {
        var jsonWithInvalidMonth = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "InvalidMonth",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithInvalidMonth);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("Invalid month", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithInvalidYear_ThrowsInvalidOperationException()
    {
        var jsonWithInvalidYear = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 1999,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithInvalidYear);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("Invalid year", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithInvalidMilestoneDate_ThrowsInvalidOperationException()
    {
        var jsonWithInvalidMilestoneDate = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Test Milestone",
              "date": "invalid-date",
              "type": "checkpoint"
            }
          ]
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithInvalidMilestoneDate);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("invalid date", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithInvalidMilestoneType_ThrowsInvalidOperationException()
    {
        var jsonWithInvalidType = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Test Milestone",
              "date": "2026-03-15",
              "type": "invalid-type"
            }
          ]
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithInvalidType);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("invalid type", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_WithDuplicateMilestoneIds_ThrowsInvalidOperationException()
    {
        var jsonWithDuplicateIds = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Milestone 1",
              "date": "2026-03-15",
              "type": "checkpoint"
            },
            {
              "id": "m1",
              "label": "Milestone 2",
              "date": "2026-04-15",
              "type": "checkpoint"
            }
          ]
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, jsonWithDuplicateIds);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetDashboardConfigAsync());

        Assert.Contains("Duplicate milestone id", exception.Message);
    }

    [Fact]
    public async Task GetDashboardConfigAsync_CachesResult()
    {
        var validJson = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, validJson);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);
        var config1 = await service.GetDashboardConfigAsync();
        var config2 = await service.GetDashboardConfigAsync();

        Assert.Same(config1, config2);
    }

    [Fact]
    public async Task GetLastModifiedTime_ReturnsFileModificationTime()
    {
        var validJson = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, validJson);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);
        await service.GetDashboardConfigAsync();

        var lastModified = service.GetLastModifiedTime();

        Assert.True(lastModified > DateTime.MinValue);
        Assert.True(lastModified <= DateTime.UtcNow);
    }

    [Fact]
    public async Task RefreshAsync_ClearsCache()
    {
        var validJson = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": []
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, validJson);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);
        var config1 = await service.GetDashboardConfigAsync();
        await service.RefreshAsync();
        var config2 = await service.GetDashboardConfigAsync();

        Assert.NotSame(config1, config2);
        Assert.Equal(config1.ProjectName, config2.ProjectName);
    }

    [Theory]
    [InlineData("poc")]
    [InlineData("release")]
    [InlineData("checkpoint")]
    public async Task GetDashboardConfigAsync_WithValidMilestoneTypes_Succeeds(string milestoneType)
    {
        var validJson = $$"""
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Test Milestone",
              "date": "2026-03-15",
              "type": "{{milestoneType}}"
            }
          ]
        }
        """;

        await File.WriteAllTextAsync(_dataFilePath, validJson);

        var service = new DashboardDataService(_configurationMock.Object, _loggerMock.Object);
        var config = await service.GetDashboardConfigAsync();

        Assert.NotNull(config);
        Assert.Equal(milestoneType, config.Milestones[0].Type);
    }
}