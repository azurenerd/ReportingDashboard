using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class ProjectDataServiceTests : IDisposable
{
    private readonly ProjectDataService service;
    private readonly ILogger<ProjectDataService> logger;
    private readonly IWebHostEnvironment environment;
    private readonly string testDataPath;
    private readonly string testDirectory;

    public ProjectDataServiceTests()
    {
        testDirectory = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}");
        Directory.CreateDirectory(testDirectory);
        testDataPath = Path.Combine(testDirectory, "data.json");

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        logger = loggerFactory.CreateLogger<ProjectDataService>();

        var mockEnvironment = new MockWebHostEnvironment { WebRootPath = testDirectory };
        environment = mockEnvironment;
        service = new ProjectDataService(logger, environment);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
    {
        // Arrange
        var validJson = @"{
            ""projectName"": ""Q2 Mobile App"",
            ""projectDescription"": ""Mobile app release"",
            ""startDate"": ""2024-01-01"",
            ""endDate"": ""2024-06-30"",
            ""completionPercentage"": 75,
            ""tasks"": [
                {
                    ""id"": ""task1"",
                    ""name"": ""API Integration"",
                    ""owner"": ""John Doe"",
                    ""status"": 0,
                    ""description"": ""Integrate backend API"",
                    ""createdDate"": ""2024-01-15""
                }
            ],
            ""milestones"": []
        }";
        File.WriteAllText(testDataPath, validJson);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Q2 Mobile App", result.ProjectName);
        Assert.Equal("Mobile app release", result.ProjectDescription);
        Assert.Equal(75, result.CompletionPercentage);
        Assert.Single(result.Tasks);
        Assert.Equal("API Integration", result.Tasks[0].Name);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingFile_ReturnsNull()
    {
        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        File.WriteAllText(testDataPath, "{invalid json");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoadProjectDataAsync()
        );
        Assert.Contains("Invalid JSON format", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithEmptyFile_ThrowsInvalidOperationException()
    {
        // Arrange
        File.WriteAllText(testDataPath, string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoadProjectDataAsync()
        );
        Assert.Contains("could not be deserialized", exception.Message);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMissingProjectName_ReturnsDataWithEmptyName()
    {
        // Arrange
        var jsonWithoutName = @"{
            ""projectDescription"": ""Test"",
            ""startDate"": ""2024-01-01"",
            ""endDate"": ""2024-06-30"",
            ""completionPercentage"": 50,
            ""tasks"": [],
            ""milestones"": []
        }";
        File.WriteAllText(testDataPath, jsonWithoutName);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.ProjectName);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMultipleTasks_LoadsAllTasks()
    {
        // Arrange
        var jsonWithMultipleTasks = @"{
            ""projectName"": ""Test Project"",
            ""projectDescription"": """",
            ""startDate"": ""2024-01-01"",
            ""endDate"": ""2024-06-30"",
            ""completionPercentage"": 60,
            ""tasks"": [
                { ""id"": ""1"", ""name"": ""Task 1"", ""owner"": ""Alice"", ""status"": 0, ""description"": """", ""createdDate"": ""2024-01-01"" },
                { ""id"": ""2"", ""name"": ""Task 2"", ""owner"": ""Bob"", ""status"": 1, ""description"": """", ""createdDate"": ""2024-01-02"" },
                { ""id"": ""3"", ""name"": ""Task 3"", ""owner"": ""Charlie"", ""status"": 2, ""description"": """", ""createdDate"": ""2024-01-03"" }
            ],
            ""milestones"": []
        }";
        File.WriteAllText(testDataPath, jsonWithMultipleTasks);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Tasks.Count);
        Assert.Equal("Task 1", result.Tasks[0].Name);
        Assert.Equal("Task 3", result.Tasks[2].Name);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMilestones_LoadsMilestoneData()
    {
        // Arrange
        var jsonWithMilestones = @"{
            ""projectName"": ""Test"",
            ""projectDescription"": """",
            ""startDate"": ""2024-01-01"",
            ""endDate"": ""2024-06-30"",
            ""completionPercentage"": 0,
            ""tasks"": [],
            ""milestones"": [
                { ""id"": ""m1"", ""name"": ""Phase 1"", ""targetDate"": ""2024-02-01"", ""status"": 0, ""completionPercentage"": 0 },
                { ""id"": ""m2"", ""name"": ""Phase 2"", ""targetDate"": ""2024-04-01"", ""status"": 1, ""completionPercentage"": 50 }
            ]
        }";
        File.WriteAllText(testDataPath, jsonWithMilestones);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Milestones.Count);
        Assert.Equal("Phase 1", result.Milestones[0].Name);
        Assert.Equal(MilestoneStatus.InProgress, result.Milestones[1].Status);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithZeroCompletionPercentage_ReturnsValidData()
    {
        // Arrange
        var json = @"{
            ""projectName"": ""Test"",
            ""projectDescription"": """",
            ""startDate"": ""2024-01-01"",
            ""endDate"": ""2024-06-30"",
            ""completionPercentage"": 0,
            ""tasks"": [],
            ""milestones"": []
        }";
        File.WriteAllText(testDataPath, json);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.CompletionPercentage);
    }

    [Fact]
    public async Task LoadProjectDataAsync_WithMaxCompletionPercentage_ReturnsValidData()
    {
        // Arrange
        var json = @"{
            ""projectName"": ""Test"",
            ""projectDescription"": """",
            ""startDate"": ""2024-01-01"",
            ""endDate"": ""2024-06-30"",
            ""completionPercentage"": 100,
            ""tasks"": [],
            ""milestones"": []
        }";
        File.WriteAllText(testDataPath, json);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.CompletionPercentage);
    }

    [Fact]
    public async Task LoadProjectDataAsync_CaseInsensitiveJson_ReturnsProjectData()
    {
        // Arrange - JSON with different casing
        var jsonWithDifferentCasing = @"{
            ""ProjectName"": ""Test"",
            ""ProjectDescription"": """",
            ""StartDate"": ""2024-01-01"",
            ""EndDate"": ""2024-06-30"",
            ""CompletionPercentage"": 50,
            ""Tasks"": [],
            ""Milestones"": []
        }";
        File.WriteAllText(testDataPath, jsonWithDifferentCasing);

        // Act
        var result = await service.LoadProjectDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.ProjectName);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, recursive: true);
            }
        }
        catch
        {
            // Cleanup best effort
        }
    }

    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public string WebRootFileProvider { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Test";
    }
}