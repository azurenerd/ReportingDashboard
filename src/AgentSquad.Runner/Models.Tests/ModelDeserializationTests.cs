using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

public class ModelDeserializationTests
{
    [Fact]
    public void TestProjectJsonDeserialization_ValidData_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test Project"",
            ""description"": ""A test project"",
            ""startDate"": ""2024-01-01"",
            ""targetEndDate"": ""2024-12-31"",
            ""completionPercentage"": 50,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 12,
            ""milestones"": [],
            ""workItems"": []
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("A test project", project.Description);
        Assert.Equal(50, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(12, project.VelocityThisMonth);
        Assert.Empty(project.Milestones);
        Assert.Empty(project.WorkItems);
    }

    [Fact]
    public void TestMilestoneJsonDeserialization_ValidData_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""name"": ""Phase 1"",
            ""targetDate"": ""2024-06-30"",
            ""status"": ""InProgress"",
            ""description"": ""Initial launch phase""
        }";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Equal("Phase 1", milestone.Name);
        Assert.Equal(new DateTime(2024, 6, 30), milestone.TargetDate);
        Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
        Assert.Equal("Initial launch phase", milestone.Description);
    }

    [Fact]
    public void TestWorkItemJsonDeserialization_ValidData_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""title"": ""Implement API"",
            ""description"": ""Build REST API endpoints"",
            ""status"": ""InProgress"",
            ""assignedTo"": ""Team A""
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Equal("Implement API", workItem.Title);
        Assert.Equal("Build REST API endpoints", workItem.Description);
        Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
        Assert.Equal("Team A", workItem.AssignedTo);
    }

    [Fact]
    public void TestProjectMetricsJsonDeserialization_ValidData_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""completionPercentage"": 75,
            ""healthStatus"": ""AtRisk"",
            ""velocityThisMonth"": 8,
            ""totalMilestones"": 5,
            ""completedMilestones"": 3
        }";

        // Act
        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(75, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
        Assert.Equal(8, metrics.VelocityThisMonth);
        Assert.Equal(5, metrics.TotalMilestones);
        Assert.Equal(3, metrics.CompletedMilestones);
    }
}