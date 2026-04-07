using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class ProjectTests
{
    [Fact]
    public void Project_Deserialize_WithCompleteJson_ReturnsCorrectObject()
    {
        // Arrange
        var json = """
        {
            "name": "Executive Dashboard",
            "description": "Project dashboard for executives",
            "startDate": "2024-01-15T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 45,
            "healthStatus": "OnTrack",
            "velocityThisMonth": 12,
            "milestones": [
                {
                    "name": "Phase 1",
                    "targetDate": "2024-03-31T00:00:00Z",
                    "status": "Completed"
                }
            ],
            "workItems": []
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var project = JsonSerializer.Deserialize<Project>(json, options);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Executive Dashboard", project.Name);
        Assert.Equal("Project dashboard for executives", project.Description);
        Assert.Equal(45, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(12, project.VelocityThisMonth);
        Assert.Single(project.Milestones);
        Assert.Empty(project.WorkItems);
    }

    [Fact]
    public void Project_Deserialize_WithMultipleMilestonesAndWorkItems_ReturnsCollections()
    {
        // Arrange
        var json = """
        {
            "name": "Complex Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 60,
            "healthStatus": "AtRisk",
            "velocityThisMonth": 8,
            "milestones": [
                {
                    "name": "Phase 1",
                    "targetDate": "2024-03-31T00:00:00Z",
                    "status": "Completed"
                },
                {
                    "name": "Phase 2",
                    "targetDate": "2024-06-30T00:00:00Z",
                    "status": "InProgress"
                }
            ],
            "workItems": [
                {
                    "title": "Task 1",
                    "status": "Shipped"
                },
                {
                    "title": "Task 2",
                    "status": "InProgress"
                }
            ]
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var project = JsonSerializer.Deserialize<Project>(json, options);

        // Assert
        Assert.Equal(2, project.Milestones.Count);
        Assert.Equal(2, project.WorkItems.Count);
    }

    [Theory]
    [InlineData("OnTrack")]
    [InlineData("AtRisk")]
    [InlineData("Blocked")]
    public void Project_DeserializeAllHealthStatuses_CorrectlyMapsEnumValues(string healthStatus)
    {
        // Arrange
        var json = $"""
        {{
            "name": "Test Project",
            "startDate": "2024-01-01T00:00:00Z",
            "targetEndDate": "2024-12-31T00:00:00Z",
            "completionPercentage": 50,
            "healthStatus": "{healthStatus}",
            "velocityThisMonth": 5,
            "milestones": [
                {{
                    "name": "M1",
                    "targetDate": "2024-06-30T00:00:00Z",
                    "status": "Future"
                }}
            ]
        }}
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var project = JsonSerializer.Deserialize<Project>(json, options);

        // Assert
        var expectedStatus = Enum.Parse<HealthStatus>(healthStatus);
        Assert.Equal(expectedStatus, project.HealthStatus);
    }

    [Fact]
    public void Project_Initialize_WithDefaultValues_CreatesEmptyCollections()
    {
        // Act
        var project = new Project();

        // Assert
        Assert.NotNull(project.Milestones);
        Assert.Empty(project.Milestones);
        Assert.NotNull(project.WorkItems);
        Assert.Empty(project.WorkItems);
    }
}