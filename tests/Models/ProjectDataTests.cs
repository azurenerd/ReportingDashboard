using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class ProjectDataTests
{
    [Fact]
    public void ProjectData_NewInstance_HasEmptyCollections()
    {
        // Act
        var projectData = new ProjectData();

        // Assert
        Assert.Equal(string.Empty, projectData.ProjectName);
        Assert.Equal(string.Empty, projectData.ProjectDescription);
        Assert.Empty(projectData.Tasks);
        Assert.Empty(projectData.Milestones);
    }

    [Fact]
    public void ProjectData_WithProperties_StoresValuesCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 6, 30);
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped }
        };
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Name = "Phase 1", Status = MilestoneStatus.Completed }
        };

        // Act
        var projectData = new ProjectData
        {
            ProjectName = "Q2 Mobile App",
            ProjectDescription = "Mobile application for Q2 release",
            StartDate = startDate,
            EndDate = endDate,
            CompletionPercentage = 75,
            Tasks = tasks,
            Milestones = milestones
        };

        // Assert
        Assert.Equal("Q2 Mobile App", projectData.ProjectName);
        Assert.Equal("Mobile application for Q2 release", projectData.ProjectDescription);
        Assert.Equal(startDate, projectData.StartDate);
        Assert.Equal(endDate, projectData.EndDate);
        Assert.Equal(75, projectData.CompletionPercentage);
        Assert.Single(projectData.Tasks);
        Assert.Single(projectData.Milestones);
    }
}