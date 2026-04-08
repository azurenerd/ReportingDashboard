using Xunit;
using AgentSquad.Dashboard.Services;

namespace AgentSquad.Tests.Models;

public class ProjectDataModelTests
{
    #region ProjectMetrics Tests

    [Fact]
    public void ProjectMetrics_CompletionPercentage_CalculatesCorrectly()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5
        };

        Assert.Equal(50, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_WithZeroTotalTasks_ReturnsZero()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 0,
            CompletedTasks = 0
        };

        Assert.Equal(0, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_WithAllTasksComplete_Returns100()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 10
        };

        Assert.Equal(100, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_WithOneTask_CalculatesCorrectly()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 1,
            CompletedTasks = 1
        };

        Assert.Equal(100, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_WithPartialCompletion_RoundsDown()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 3,
            CompletedTasks = 1
        };

        Assert.Equal(33, metrics.CompletionPercentage);
    }

    #endregion

    #region ProjectInfo Tests

    [Fact]
    public void ProjectInfo_DefaultValues_InitializedCorrectly()
    {
        var project = new ProjectInfo();

        Assert.Equal(string.Empty, project.Name);
        Assert.Equal(string.Empty, project.Description);
        Assert.Equal(string.Empty, project.Status);
        Assert.Equal(string.Empty, project.Sponsor);
        Assert.Equal(string.Empty, project.ProjectManager);
    }

    [Fact]
    public void ProjectInfo_WithValues_StoresCorrectly()
    {
        var project = new ProjectInfo
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 12, 31),
            Status = "OnTrack",
            Sponsor = "Executive",
            ProjectManager = "Manager Name"
        };

        Assert.Equal("Test Project", project.Name);
        Assert.Equal("Test Description", project.Description);
        Assert.Equal("OnTrack", project.Status);
        Assert.Equal("Executive", project.Sponsor);
        Assert.Equal("Manager Name", project.ProjectManager);
    }

    #endregion

    #region Milestone Tests

    [Fact]
    public void Milestone_DefaultValues_InitializedCorrectly()
    {
        var milestone = new Milestone();

        Assert.Equal(string.Empty, milestone.Id);
        Assert.Equal(string.Empty, milestone.Name);
        Assert.Null(milestone.ActualDate);
    }

    [Fact]
    public void Milestone_CompletedStatus_WithActualDate()
    {
        var milestone = new Milestone
        {
            Id = "m1",
            Name = "Design",
            TargetDate = new DateTime(2024, 2, 15),
            ActualDate = new DateTime(2024, 2, 14),
            Status = MilestoneStatus.Completed,
            CompletionPercentage = 100
        };

        Assert.NotNull(milestone.ActualDate);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
    }

    [Fact]
    public void Milestone_InProgressStatus_WithoutActualDate()
    {
        var milestone = new Milestone
        {
            Id = "m2",
            Name = "Development",
            TargetDate = new DateTime(2024, 4, 30),
            Status = MilestoneStatus.InProgress,
            CompletionPercentage = 50
        };

        Assert.Null(milestone.ActualDate);
        Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
        Assert.Equal(50, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_PendingStatus_HasZeroCompletion()
    {
        var milestone = new Milestone
        {
            Id = "m3",
            Name = "Testing",
            TargetDate = new DateTime(2024, 6, 30),
            Status = MilestoneStatus.Pending,
            CompletionPercentage = 0
        };

        Assert.Equal(MilestoneStatus.Pending, milestone.Status);
        Assert.Equal(0, milestone.CompletionPercentage);
    }

    #endregion

    #region Task Tests

    [Fact]
    public void Task_DefaultValues_InitializedCorrectly()
    {
        var task = new Task();

        Assert.Equal(string.Empty, task.Id);
        Assert.Equal(string.Empty, task.Name);
        Assert.Equal(string.Empty, task.AssignedTo);
        Assert.Equal(string.Empty, task.RelatedMilestone);
    }

    [Fact]
    public void Task_ShippedStatus_StoresCorrectly()
    {
        var task = new Task
        {
            Id = "t1",
            Name = "API Integration",
            Status = TaskStatus.Shipped,
            AssignedTo = "Bob",
            DueDate = new DateTime(2024, 3, 1),
            EstimatedDays = 5,
            RelatedMilestone = "m1"
        };

        Assert.Equal(TaskStatus.Shipped, task.Status);
        Assert.Equal("Bob", task.AssignedTo);
        Assert.Equal(5, task.EstimatedDays);
    }

    [Fact]
    public void Task_InProgressStatus_StoresCorrectly()
    {
        var task = new Task
        {
            Id = "t2",
            Name = "UI Work",
            Status = TaskStatus.InProgress,
            AssignedTo = "Carol",
            DueDate = new DateTime(2024, 4, 15),
            EstimatedDays = 10,
            RelatedMilestone = "m2"
        };

        Assert.Equal(TaskStatus.InProgress, task.Status);
    }

    [Fact]
    public void Task_CarriedOverStatus_StoresCorrectly()
    {
        var task = new Task
        {
            Id = "t3",
            Name = "Testing",
            Status = TaskStatus.CarriedOver,
            AssignedTo = "David",
            DueDate = new DateTime(2024, 5, 1),
            EstimatedDays = 7,
            RelatedMilestone = "m2"
        };

        Assert.Equal(TaskStatus.CarriedOver, task.Status);
    }

    [Fact]
    public void Task_WithoutAssignee_StoresEmptyString()
    {
        var task = new Task
        {
            Id = "t4",
            Name = "Unassigned",
            Status = TaskStatus.InProgress,
            AssignedTo = "",
            DueDate = new DateTime(2024, 4, 15),
            EstimatedDays = 5,
            RelatedMilestone = "m2"
        };

        Assert.Equal("", task.AssignedTo);
    }

    #endregion

    #region ProjectData Tests

    [Fact]
    public void ProjectData_DefaultValues_InitializedCorrectly()
    {
        var data = new ProjectData();

        Assert.Null(data.Project);
        Assert.Empty(data.Milestones);
        Assert.Empty(data.Tasks);
        Assert.Null(data.Metrics);
    }

    [Fact]
    public void ProjectData_WithCompleteData_StoresCorrectly()
    {
        var data = new ProjectData
        {
            Project = new ProjectInfo { Name = "Test" },
            Milestones = new List<Milestone> { new() },
            Tasks = new List<Task> { new() },
            Metrics = new ProjectMetrics { TotalTasks = 1 }
        };

        Assert.NotNull(data.Project);
        Assert.Single(data.Milestones);
        Assert.Single(data.Tasks);
        Assert.NotNull(data.Metrics);
    }

    #endregion
}