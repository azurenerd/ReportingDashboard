using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Tests for JSON serialization and deserialization across all model types.
/// Validates camelCase property naming and System.Text.Json compatibility.
/// </summary>
public class JsonSerializationTests
{
    [Fact]
    public void JsonDeserialization_CamelCaseProperties_CorrectlyMapped()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test Project"",
            ""completionPercentage"": 45,
            ""velocityThisMonth"": 12,
            ""healthStatus"": ""OnTrack""
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal(45, project.CompletionPercentage);
        Assert.Equal(12, project.VelocityThisMonth);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
    }

    [Fact]
    public void JsonSerialization_ProducesValidJson()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test",
            StartDate = new DateTime(2024, 1, 1),
            TargetEndDate = new DateTime(2024, 12, 31)
        };

        // Act
        var json = JsonSerializer.Serialize(project);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"name\":", json);
        Assert.Contains("\"Test\"", json);
    }

    [Fact]
    public void JsonRoundtrip_ProjectWithNestedCollections()
    {
        // Arrange
        var original = new Project
        {
            Name = "Dashboard",
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = new DateTime(2024, 3, 31) }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem { Title = "W1", Status = WorkItemStatus.Shipped }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.Single(restored.Milestones);
        Assert.Single(restored.WorkItems);
        Assert.Equal("M1", restored.Milestones[0].Name);
        Assert.Equal("W1", restored.WorkItems[0].Title);
    }

    [Fact]
    public void JsonDeserialization_EmptyCollections()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test"",
            ""milestones"": [],
            ""workItems"": []
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(project);
        Assert.Empty(project.Milestones);
        Assert.Empty(project.WorkItems);
    }

    [Fact]
    public void JsonDeserialization_MalformedStatus_Throws()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test"",
            ""targetDate"": ""2024-06-30"",
            ""status"": ""InvalidStatus"",
            ""description"": ""Test""
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Milestone>(json));
    }

    [Fact]
    public void JsonDeserialization_InvalidDateFormat_Throws()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test Project"",
            ""startDate"": ""not-a-date"",
            ""targetEndDate"": ""2024-12-31""
        }";

        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Project>(json));
    }

    [Fact]
    public void JsonDeserialization_MissingRequiredField_UsesDefault()
    {
        // Arrange - omit optional description
        var json = @"{
            ""name"": ""Test"",
            ""targetDate"": ""2024-06-30"",
            ""status"": ""InProgress""
        }";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Null(milestone.Description);
    }

    [Fact]
    public void JsonDeserialization_DateTimeFormats_ISO8601Supported()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test"",
            ""startDate"": ""2024-01-15T10:30:00Z"",
            ""targetEndDate"": ""2024-12-31T23:59:59Z""
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(project);
        Assert.True(project.StartDate.Year == 2024);
        Assert.True(project.TargetEndDate.Year == 2024);
    }

    [Fact]
    public void JsonDeserialization_LargeNumbers_SupportedForMetrics()
    {
        // Arrange
        var json = @"{
            ""completionPercentage"": 99,
            ""velocityThisMonth"": 9999,
            ""totalMilestones"": 500,
            ""completedMilestones"": 250
        }";

        // Act
        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json);

        // Assert
        Assert.Equal(99, metrics.CompletionPercentage);
        Assert.Equal(9999, metrics.VelocityThisMonth);
        Assert.Equal(500, metrics.TotalMilestones);
    }

    [Fact]
    public void JsonDeserialization_SpecialCharactersInStrings()
    {
        // Arrange
        var json = @"{
            ""name"": ""Project: ""Test"" & <Success>"",
            ""description"": ""Description with \""quotes\""and\\backslashes""
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(project);
        Assert.Contains("Test", project.Name);
    }
}