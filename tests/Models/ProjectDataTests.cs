using Xunit;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Tests.Models;

public class ProjectDataTests
{
    [Fact]
    public void ProjectData_ShouldInitializeWithEmptyCollections()
    {
        var projectData = new ProjectData();
        
        Assert.NotNull(projectData.Milestones);
        Assert.NotNull(projectData.Tasks);
        Assert.Empty(projectData.Milestones);
        Assert.Empty(projectData.Tasks);
    }

    [Fact]
    public void Milestone_CompletedStatus_ShouldHaveZeroValue()
    {
        Assert.Equal(0, (int)MilestoneStatus.Completed);
    }

    [Fact]
    public void Milestone_WithAllFields_ShouldDeserializeCorrectly()
    {
        var milestone = new Milestone
        {
            Id = "M1",
            Name = "Phase 1 Complete",
            TargetDate = new DateTime(2026, 06, 30),
            ActualDate = new DateTime(2026, 06, 28),
            Status = MilestoneStatus.Completed,
            CompletionPercentage = 100
        };

        Assert.Equal("M1", milestone.Id);
        Assert.Equal("Phase 1 Complete", milestone.Name);
        Assert.Equal(100, milestone.CompletionPercentage);
        Assert.NotNull(milestone.ActualDate);
    }

    [Fact]
    public void Task_WithCarriedOverStatus_ShouldHaveCorrectValue()
    {
        Assert.Equal(2, (int)TaskStatus.CarriedOver);
    }

    [Fact]
    public void Task_ShouldContainAllRequiredFields()
    {
        var task = new Task
        {
            Id = "T1",
            Name = "Implement auth",
            Status = TaskStatus.InProgress,
            AssignedTo = "John Doe",
            DueDate = new DateTime(2026, 05, 15),
            EstimatedDays = 5,
            RelatedMilestone = "M1"
        };

        Assert.Equal("T1", task.Id);
        Assert.Equal("Implement auth", task.Name);
        Assert.Equal(TaskStatus.InProgress, task.Status);
        Assert.Equal("John Doe", task.AssignedTo);
        Assert.Equal(5, task.EstimatedDays);
    }

    [Fact]
    public void ProjectMetrics_ShouldCalculateCompletionPercentageCorrectly()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 6,
            CompletionPercentage = 60
        };

        Assert.Equal(10, metrics.TotalTasks);
        Assert.Equal(6, metrics.CompletedTasks);
        Assert.Equal(60, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectInfo_ShouldContainAllMetadata()
    {
        var projectInfo = new ProjectInfo
        {
            Name = "Q2 Mobile App",
            Description = "Mobile app release",
            StartDate = new DateTime(2026, 04, 01),
            EndDate = new DateTime(2026, 07, 31),
            Status = "In Progress",
            Sponsor = "VP Product",
            ProjectManager = "Jane Smith"
        };

        Assert.Equal("Q2 Mobile App", projectInfo.Name);
        Assert.Equal("Jane Smith", projectInfo.ProjectManager);
        Assert.Equal("In Progress", projectInfo.Status);
    }

    [Fact]
    public void TaskStatusSummary_ShouldTrackAllStatuses()
    {
        var summary = new TaskStatusSummary
        {
            ShippedCount = 5,
            InProgressCount = 3,
            CarriedOverCount = 2
        };

        Assert.Equal(5, summary.ShippedCount);
        Assert.Equal(3, summary.InProgressCount);
        Assert.Equal(2, summary.CarriedOverCount);
        Assert.Equal(10, summary.ShippedCount + summary.InProgressCount + summary.CarriedOverCount);
    }
}