using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

public class ModelNullSafetyTests
{
    [Fact]
    public void TestProjectJsonDeserialization_NullDescription_HandlesGracefully()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test"",
            ""description"": null,
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
        Assert.Null(project.Description);
    }

    [Fact]
    public void TestWorkItemJsonDeserialization_NullOptionalFields_HandlesGracefully()
    {
        // Arrange
        var json = @"{
            ""title"": ""Task"",
            ""description"": null,
            ""status"": ""Shipped"",
            ""assignedTo"": null
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Null(workItem.Description);
        Assert.Null(workItem.AssignedTo);
    }

    [Fact]
    public void TestMilestoneJsonDeserialization_NullDescription_HandlesGracefully()
    {
        // Arrange
        var json = @"{
            ""name"": ""Milestone"",
            ""targetDate"": ""2024-06-30"",
            ""status"": ""Completed"",
            ""description"": null
        }";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Null(milestone.Description);
    }
}