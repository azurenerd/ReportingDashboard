using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

public class IntegrationSmokeTests
{
    [Fact]
    public void TestFullProjectDeserialization_SampleData_LoadsSuccessfully()
    {
        // Arrange
        var sampleJson = @"{
            ""name"": ""Executive Dashboard Project"",
            ""description"": ""Build real-time project dashboard"",
            ""startDate"": ""2024-01-15"",
            ""targetEndDate"": ""2024-12-31"",
            ""completionPercentage"": 45,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 12,
            ""milestones"": [
                {
                    ""name"": ""Phase 1 Launch"",
                    ""targetDate"": ""2024-03-31"",
                    ""status"": ""Completed"",
                    ""description"": ""Core feature rollout""
                },
                {
                    ""name"": ""Phase 2 Optimization"",
                    ""targetDate"": ""2024-06-30"",
                    ""status"": ""InProgress"",
                    ""description"": ""Performance improvements""
                },
                {
                    ""name"": ""Phase 3 Polish"",
                    ""targetDate"": ""2024-12-31"",
                    ""status"": ""Future"",
                    ""description"": ""Final refinements""
                }
            ],
            ""workItems"": [
                {
                    ""title"": ""API Integration"",
                    ""description"": ""Connect to external data source"",
                    ""status"": ""Shipped"",
                    ""assignedTo"": ""Team A""
                },
                {
                    ""title"": ""Dashboard UI"",
                    ""description"": ""Build Blazor components"",
                    ""status"": ""InProgress"",
                    ""assignedTo"": ""Team B""
                },
                {
                    ""title"": ""Performance Testing"",
                    ""description"": ""Load and stress testing"",
                    ""status"": ""CarriedOver"",
                    ""assignedTo"": ""Team C""
                }
            ]
        }";

        // Act
        var project = JsonSerializer.Deserialize<Project>(sampleJson);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Executive Dashboard Project", project.Name);
        Assert.Equal("Build real-time project dashboard", project.Description);
        Assert.Equal(45, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(12, project.VelocityThisMonth);
        
        // Verify milestones
        Assert.Equal(3, project.Milestones.Count);
        Assert.Equal("Phase 1 Launch", project.Milestones[0].Name);
        Assert.Equal(MilestoneStatus.Completed, project.Milestones[0].Status);
        Assert.Equal("Phase 2 Optimization", project.Milestones[1].Name);
        Assert.Equal(MilestoneStatus.InProgress, project.Milestones[1].Status);
        Assert.Equal("Phase 3 Polish", project.Milestones[2].Name);
        Assert.Equal(MilestoneStatus.Future, project.Milestones[2].Status);
        
        // Verify work items
        Assert.Equal(3, project.WorkItems.Count);
        Assert.Equal("API Integration", project.WorkItems[0].Title);
        Assert.Equal(WorkItemStatus.Shipped, project.WorkItems[0].Status);
        Assert.Equal("Dashboard UI", project.WorkItems[1].Title);
        Assert.Equal(WorkItemStatus.InProgress, project.WorkItems[1].Status);
        Assert.Equal("Performance Testing", project.WorkItems[2].Title);
        Assert.Equal(WorkItemStatus.CarriedOver, project.WorkItems[2].Status);
    }

    [Fact]
    public void TestChildCollectionsInitialized_NoNullReferenceExceptions()
    {
        // Arrange
        var project = new Project();

        // Act & Assert - Should not throw NullReferenceException
        Assert.NotNull(project.Milestones);
        Assert.NotNull(project.WorkItems);
        Assert.Empty(project.Milestones);
        Assert.Empty(project.WorkItems);
    }

    [Fact]
    public void TestModelPropertiesAccessible_NoExceptions()
    {
        // Arrange
        var milestone = new Milestone { Name = "Test", Status = MilestoneStatus.InProgress };
        var workItem = new WorkItem { Title = "Task", Status = WorkItemStatus.InProgress };
        var metrics = new ProjectMetrics { CompletionPercentage = 50 };

        // Act & Assert - Verify all properties are accessible
        Assert.Equal("Test", milestone.Name);
        Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
        Assert.Equal("Task", workItem.Title);
        Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
        Assert.Equal(50, metrics.CompletionPercentage);
    }
}