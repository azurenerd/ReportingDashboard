using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Integration tests simulating real data.json loading scenarios.
/// Validates complete project structure deserialization for dashboard functionality.
/// </summary>
public class DataLoadingIntegrationTests
{
    private const string SampleProjectDataJson = @"{
        ""name"": ""Q1 Executive Dashboard Initiative"",
        ""description"": ""Build real-time project visibility platform for executives"",
        ""startDate"": ""2024-01-01"",
        ""targetEndDate"": ""2024-12-31"",
        ""completionPercentage"": 42,
        ""healthStatus"": ""OnTrack"",
        ""velocityThisMonth"": 12,
        ""milestones"": [
            {
                ""name"": ""Phase 1: Architecture & Setup"",
                ""targetDate"": ""2024-03-31"",
                ""status"": ""Completed"",
                ""description"": ""Blazor Server project structure and foundation components""
            },
            {
                ""name"": ""Phase 2: Dashboard Components"",
                ""targetDate"": ""2024-06-30"",
                ""status"": ""InProgress"",
                ""description"": ""Implement timeline, metrics, and work item display components""
            },
            {
                ""name"": ""Phase 3: Polish & Optimization"",
                ""targetDate"": ""2024-09-30"",
                ""status"": ""Future"",
                ""description"": ""Performance optimization and UI refinements""
            },
            {
                ""name"": ""Phase 4: Documentation"",
                ""targetDate"": ""2024-12-31"",
                ""status"": ""Future"",
                ""description"": ""User guide and deployment documentation""
            }
        ],
        ""workItems"": [
            {
                ""title"": ""API Integration"",
                ""description"": ""Implement REST endpoints for data retrieval"",
                ""status"": ""Shipped"",
                ""assignedTo"": ""Backend Team""
            },
            {
                ""title"": ""Dashboard UI Components"",
                ""description"": ""Build Blazor components for visualization"",
                ""status"": ""InProgress"",
                ""assignedTo"": ""Frontend Team""
            },
            {
                ""title"": ""Performance Testing"",
                ""description"": ""Execute load and stress testing scenarios"",
                ""status"": ""CarriedOver"",
                ""assignedTo"": ""QA Team""
            },
            {
                ""title"": ""Database Optimization"",
                ""description"": ""Improve query performance for large datasets"",
                ""status"": ""InProgress"",
                ""assignedTo"": ""Database Team""
            },
            {
                ""title"": ""Security Audit"",
                ""description"": ""Conduct security review and remediate issues"",
                ""status"": ""CarriedOver"",
                ""assignedTo"": ""Security Team""
            }
        ]
    }";

    [Fact]
    public void LoadProjectData_SampleJson_DeserializesSuccessfully()
    {
        // Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Q1 Executive Dashboard Initiative", project.Name);
    }

    [Fact]
    public void LoadProjectData_AllMilestonesPopulated()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.Equal(4, project.Milestones.Count);
        Assert.All(project.Milestones, m => Assert.NotNull(m.Name));
        Assert.All(project.Milestones, m => Assert.NotEqual(default(MilestoneStatus), m.Status));
    }

    [Fact]
    public void LoadProjectData_MilestoneStatuses_CorrectlyAssigned()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.Equal(MilestoneStatus.Completed, project.Milestones[0].Status);
        Assert.Equal(MilestoneStatus.InProgress, project.Milestones[1].Status);
        Assert.Equal(MilestoneStatus.Future, project.Milestones[2].Status);
        Assert.Equal(MilestoneStatus.Future, project.Milestones[3].Status);
    }

    [Fact]
    public void LoadProjectData_AllWorkItemsPopulated()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.Equal(5, project.WorkItems.Count);
        Assert.All(project.WorkItems, w => Assert.NotNull(w.Title));
        Assert.All(project.WorkItems, w => Assert.NotNull(w.AssignedTo));
    }

    [Fact]
    public void LoadProjectData_WorkItemStatuses_CorrectlyAssigned()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.Equal(WorkItemStatus.Shipped, project.WorkItems[0].Status);
        Assert.Equal(WorkItemStatus.InProgress, project.WorkItems[1].Status);
        Assert.Equal(WorkItemStatus.CarriedOver, project.WorkItems[2].Status);
        Assert.Equal(WorkItemStatus.InProgress, project.WorkItems[3].Status);
        Assert.Equal(WorkItemStatus.CarriedOver, project.WorkItems[4].Status);
    }

    [Fact]
    public void LoadProjectData_ProjectMetricsAccessible()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.Equal(42, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(12, project.VelocityThisMonth);
    }

    [Fact]
    public void LoadProjectData_DateValuesCorrect()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert
        Assert.Equal(new DateTime(2024, 1, 1), project.StartDate);
        Assert.Equal(new DateTime(2024, 12, 31), project.TargetEndDate);
        Assert.True(project.Milestones.All(m => m.TargetDate.Year == 2024));
    }

    [Fact]
    public void LoadProjectData_NoNullReferences_AllPropertiesAccessible()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert - Verify no null reference exceptions
        Assert.NotNull(project.Name);
        Assert.NotNull(project.Description);
        Assert.NotEmpty(project.Milestones);
        Assert.NotEmpty(project.WorkItems);

        foreach (var milestone in project.Milestones)
        {
            Assert.NotNull(milestone.Name);
            Assert.NotNull(milestone.Description);
        }

        foreach (var workItem in project.WorkItems)
        {
            Assert.NotNull(workItem.Title);
            Assert.NotNull(workItem.AssignedTo);
        }
    }

    [Fact]
    public void LoadProjectData_StatusDistribution_MixedStates()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert - Verify variety of statuses present
        var milestoneStatuses = project.Milestones.Select(m => m.Status).Distinct().ToList();
        Assert.Contains(MilestoneStatus.Completed, milestoneStatuses);
        Assert.Contains(MilestoneStatus.InProgress, milestoneStatuses);
        Assert.Contains(MilestoneStatus.Future, milestoneStatuses);

        var workItemStatuses = project.WorkItems.Select(w => w.Status).Distinct().ToList();
        Assert.Contains(WorkItemStatus.Shipped, workItemStatuses);
        Assert.Contains(WorkItemStatus.InProgress, workItemStatuses);
        Assert.Contains(WorkItemStatus.CarriedOver, workItemStatuses);
    }

    [Fact]
    public void LoadProjectData_ComplexStructure_Roundtrip()
    {
        // Arrange
        var original = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Act
        var serialized = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<Project>(serialized);

        // Assert
        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.CompletionPercentage, restored.CompletionPercentage);
        Assert.Equal(original.Milestones.Count, restored.Milestones.Count);
        Assert.Equal(original.WorkItems.Count, restored.WorkItems.Count);
    }

    [Fact]
    public void LoadProjectData_CountMetrics_CalculableFromData()
    {
        // Arrange & Act
        var project = JsonSerializer.Deserialize<Project>(SampleProjectDataJson);

        // Assert - Verify metrics can be calculated
        var shippedCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.Shipped);
        var inProgressCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
        var carriedOverCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver);

        Assert.Equal(1, shippedCount);
        Assert.Equal(2, inProgressCount);
        Assert.Equal(2, carriedOverCount);
        Assert.Equal(5, shippedCount + inProgressCount + carriedOverCount);
    }
}