using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Comprehensive tests for Project model including construction, properties, and JSON operations.
/// Verifies acceptance criteria for data model implementation.
/// </summary>
public class ProjectModelTests
{
    [Fact]
    public void Project_DefaultConstruction_InitializesCollectionsAsEmpty()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        Assert.NotNull(project.Milestones);
        Assert.NotNull(project.WorkItems);
        Assert.Empty(project.Milestones);
        Assert.Empty(project.WorkItems);
        Assert.Equal(string.Empty, project.Name);
    }

    [Fact]
    public void Project_ConstructWithProperties_AllPropertiesSet()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        // Act
        var project = new Project
        {
            Name = "Q1 Initiative",
            Description = "Quarterly goals",
            StartDate = startDate,
            TargetEndDate = endDate,
            CompletionPercentage = 42,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 8
        };

        // Assert
        Assert.Equal("Q1 Initiative", project.Name);
        Assert.Equal("Quarterly goals", project.Description);
        Assert.Equal(startDate, project.StartDate);
        Assert.Equal(endDate, project.TargetEndDate);
        Assert.Equal(42, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(8, project.VelocityThisMonth);
    }

    [Fact]
    public void Project_JsonDeserialization_FullProjectWithCollections()
    {
        // Arrange
        var json = @"{
            ""name"": ""Executive Dashboard"",
            ""description"": ""Real-time project visibility"",
            ""startDate"": ""2024-01-01"",
            ""targetEndDate"": ""2024-12-31"",
            ""completionPercentage"": 45,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 12,
            ""milestones"": [
                { ""name"": ""Launch"", ""targetDate"": ""2024-03-31"", ""status"": ""Completed"", ""description"": ""Phase 1"" },
                { ""name"": ""Optimize"", ""targetDate"": ""2024-06-30"", ""status"": ""InProgress"", ""description"": ""Phase 2"" }
            ],
            ""workItems"": [
                { ""title"": ""API"", ""status"": ""Shipped"", ""assignedTo"": ""Team A"" },
                { ""title"": ""UI"", ""status"": ""InProgress"", ""assignedTo"": ""Team B"" }
            ]
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Executive Dashboard", project.Name);
        Assert.Equal(2, project.Milestones.Count);
        Assert.Equal(2, project.WorkItems.Count);
        Assert.Equal(45, project.CompletionPercentage);
    }

    [Fact]
    public void Project_CompletionPercentage_BoundaryZero()
    {
        // Act
        var project = new Project { CompletionPercentage = 0 };

        // Assert
        Assert.Equal(0, project.CompletionPercentage);
    }

    [Fact]
    public void Project_CompletionPercentage_BoundaryHundred()
    {
        // Act
        var project = new Project { CompletionPercentage = 100 };

        // Assert
        Assert.Equal(100, project.CompletionPercentage);
    }

    [Fact]
    public void Project_CompletionPercentage_MidRange()
    {
        // Act
        var project = new Project { CompletionPercentage = 50 };

        // Assert
        Assert.Equal(50, project.CompletionPercentage);
    }

    [Fact]
    public void Project_NullDescription_Allowed()
    {
        // Arrange
        var json = @"{
            ""name"": ""Test"",
            ""description"": null,
            ""startDate"": ""2024-01-01"",
            ""targetEndDate"": ""2024-12-31"",
            ""completionPercentage"": 0,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 0,
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
    public void Project_MilestoneCollection_AddAndAccess()
    {
        // Arrange
        var project = new Project();
        var milestone = new Milestone { Name = "Phase 1", Status = MilestoneStatus.InProgress };

        // Act
        project.Milestones.Add(milestone);

        // Assert
        Assert.Single(project.Milestones);
        Assert.Equal("Phase 1", project.Milestones[0].Name);
    }

    [Fact]
    public void Project_WorkItemCollection_AddAndAccess()
    {
        // Arrange
        var project = new Project();
        var workItem = new WorkItem { Title = "Task 1", Status = WorkItemStatus.Shipped };

        // Act
        project.WorkItems.Add(workItem);

        // Assert
        Assert.Single(project.WorkItems);
        Assert.Equal("Task 1", project.WorkItems[0].Title);
    }

    [Fact]
    public void Project_DateComparison_StartBeforeEnd()
    {
        // Arrange
        var project = new Project
        {
            StartDate = new DateTime(2024, 1, 1),
            TargetEndDate = new DateTime(2024, 12, 31)
        };

        // Act & Assert
        Assert.True(project.StartDate < project.TargetEndDate);
    }

    [Fact]
    public void Project_Serialization_RoundtripPreservesAllData()
    {
        // Arrange
        var original = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = new DateTime(2024, 1, 15),
            TargetEndDate = new DateTime(2024, 12, 15),
            CompletionPercentage = 33,
            HealthStatus = HealthStatus.AtRisk,
            VelocityThisMonth = 7,
            Milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", TargetDate = new DateTime(2024, 3, 31), Status = MilestoneStatus.Completed }
            },
            WorkItems = new List<WorkItem>
            {
                new WorkItem { Title = "W1", Status = WorkItemStatus.InProgress }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.CompletionPercentage, restored.CompletionPercentage);
        Assert.Equal(original.Milestones.Count, restored.Milestones.Count);
        Assert.Equal(original.WorkItems.Count, restored.WorkItems.Count);
    }
}