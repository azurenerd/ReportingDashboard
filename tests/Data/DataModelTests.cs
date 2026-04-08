using AgentSquad.Runner.Data;
using Xunit;

namespace AgentSquad.Runner.Tests.Data;

public class DataModelTests
{
    [Fact]
    public void ProjectData_InitializesWithEmptyCollections()
    {
        // Act
        var project = new ProjectData();

        // Assert
        Assert.NotNull(project.Milestones);
        Assert.Empty(project.Milestones);
        Assert.NotNull(project.Tasks);
        Assert.Empty(project.Tasks);
    }

    [Fact]
    public void ProjectInfo_StoresProjectDetails()
    {
        // Arrange & Act
        var project = new ProjectInfo
        {
            Name = "Q2 Release",
            Description = "Mobile app launch",
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 6, 30)
        };

        // Assert
        Assert.Equal("Q2 Release", project.Name);
        Assert.Equal("Mobile app launch", project.Description);
        Assert.Equal(new DateTime(2024, 1, 1), project.StartDate);
        Assert.Equal(new DateTime(2024, 6, 30), project.EndDate);
    }

    [Fact]
    public void Milestone_StoresAllProperties()
    {
        // Arrange & Act
        var milestone = new Milestone
        {
            Id = "m1",
            Name = "Design Complete",
            TargetDate = new DateTime(2024, 2, 1),
            Status = MilestoneStatus.Completed,
            CompletionPercentage = 100
        };

        // Assert
        Assert.Equal("m1", milestone.Id);
        Assert.Equal("Design Complete", milestone.Name);
        Assert.Equal(new DateTime(2024, 2, 1), milestone.TargetDate);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
        Assert.Equal(100, milestone.CompletionPercentage);
    }

    [Fact]
    public void Task_StoresAllProperties()
    {
        // Arrange & Act
        var task = new Task
        {
            Id = "t1",
            Name = "UI Development",
            Owner = "Alice",
            Status = TaskStatus.InProgress,
            DueDate = new DateTime(2024, 3, 15)
        };

        // Assert
        Assert.Equal("t1", task.Id);
        Assert.Equal("UI Development", task.Name);
        Assert.Equal("Alice", task.Owner);
        Assert.Equal(TaskStatus.InProgress, task.Status);
        Assert.Equal(new DateTime(2024, 3, 15), task.DueDate);
    }

    [Fact]
    public void ProjectMetrics_StoresMetricsValues()
    {
        // Arrange & Act
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 75,
            TasksShipped = 3,
            TasksInProgress = 1,
            TasksCarriedOver = 1
        };

        // Assert
        Assert.Equal(75, metrics.CompletionPercentage);
        Assert.Equal(3, metrics.TasksShipped);
        Assert.Equal(1, metrics.TasksInProgress);
        Assert.Equal(1, metrics.TasksCarriedOver);
    }

    [Fact]
    public void MilestoneStatus_HasCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)MilestoneStatus.Pending);
        Assert.Equal(1, (int)MilestoneStatus.InProgress);
        Assert.Equal(2, (int)MilestoneStatus.Completed);
    }

    [Fact]
    public void TaskStatus_HasCorrectValues()
    {
        // Assert
        Assert.Equal(0, (int)TaskStatus.Shipped);
        Assert.Equal(1, (int)TaskStatus.InProgress);
        Assert.Equal(2, (int)TaskStatus.CarriedOver);
    }

    [Fact]
    public void ProjectData_CanAddMilestones()
    {
        // Arrange
        var project = new ProjectData();
        var milestone = new Milestone { Id = "m1", Name = "Phase 1", TargetDate = DateTime.Now, Status = MilestoneStatus.Pending, CompletionPercentage = 0 };

        // Act
        project.Milestones.Add(milestone);

        // Assert
        Assert.Single(project.Milestones);
        Assert.Equal("Phase 1", project.Milestones[0].Name);
    }

    [Fact]
    public void ProjectData_CanAddTasks()
    {
        // Arrange
        var project = new ProjectData();
        var task = new Task { Id = "t1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.InProgress, DueDate = DateTime.Now };

        // Act
        project.Tasks.Add(task);

        // Assert
        Assert.Single(project.Tasks);
        Assert.Equal("Task 1", project.Tasks[0].Name);
    }
}