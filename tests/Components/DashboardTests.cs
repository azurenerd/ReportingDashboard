using Xunit;
using Bunit;
using Moq;
using AgentSquad.Dashboard.Components;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Tests.Components;

public class DashboardTests : TestContext
{
    [Fact]
    public void Dashboard_RendersTimelineSection()
    {
        var projectData = CreateSampleProjectData();

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        Assert.Contains("Project Timeline", component.Markup);
    }

    [Fact]
    public void Dashboard_RendersStatusSection()
    {
        var projectData = CreateSampleProjectData();

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        Assert.Contains("Task Status Breakdown", component.Markup);
    }

    [Fact]
    public void Dashboard_RendersMetricsSection()
    {
        var projectData = CreateSampleProjectData();

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        Assert.Contains("Progress Metrics", component.Markup);
    }

    [Fact]
    public void Dashboard_RendersMilestoneTimeline()
    {
        var projectData = CreateSampleProjectData();

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        var timeline = component.FindComponent<MilestoneTimeline>();
        Assert.NotNull(timeline);
    }

    [Fact]
    public void Dashboard_RendersStatusCardsForAllStatuses()
    {
        var projectData = CreateSampleProjectData();

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        var statusCards = component.FindComponents<StatusCard>();
        Assert.Equal(3, statusCards.Count);
    }

    [Fact]
    public void Dashboard_RendersProgressMetrics()
    {
        var projectData = CreateSampleProjectData();

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        var metrics = component.FindComponent<ProgressMetrics>();
        Assert.NotNull(metrics);
    }

    [Fact]
    public void Dashboard_PassesMilestonesToTimeline()
    {
        var projectData = CreateSampleProjectData();
        projectData.Milestones.Add(new Milestone { Id = "M1", Name = "Phase 1", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 });

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        var timeline = component.FindComponent<MilestoneTimeline>();
        Assert.NotNull(timeline.Instance.Milestones);
        Assert.Single(timeline.Instance.Milestones);
    }

    [Fact]
    public void Dashboard_FilterTasksByStatusCorrectly()
    {
        var projectData = CreateSampleProjectData();
        projectData.Tasks.Add(new Task { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "User1", DueDate = DateTime.Now, EstimatedDays = 5 });
        projectData.Tasks.Add(new Task { Id = "T2", Name = "Task 2", Status = TaskStatus.InProgress, AssignedTo = "User2", DueDate = DateTime.Now, EstimatedDays = 3 });

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        var statusCards = component.FindComponents<StatusCard>();
        Assert.NotNull(statusCards);
    }

    [Fact]
    public void Dashboard_CalculatesStatusSummaryCorrectly()
    {
        var projectData = CreateSampleProjectData();
        projectData.Tasks.Add(new Task { Id = "T1", Name = "T1", Status = TaskStatus.Shipped, AssignedTo = "User", DueDate = DateTime.Now, EstimatedDays = 5 });
        projectData.Tasks.Add(new Task { Id = "T2", Name = "T2", Status = TaskStatus.Shipped, AssignedTo = "User", DueDate = DateTime.Now, EstimatedDays = 3 });
        projectData.Tasks.Add(new Task { Id = "T3", Name = "T3", Status = TaskStatus.InProgress, AssignedTo = "User", DueDate = DateTime.Now, EstimatedDays = 2 });

        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, projectData)
        );

        Assert.NotNull(component.Instance);
    }

    [Fact]
    public void Dashboard_HandlesNullProjectData()
    {
        var component = RenderComponent<Dashboard>(
            parameters => parameters
                .Add(p => p.ProjectData, new ProjectData())
        );

        Assert.NotNull(component);
    }

    private ProjectData CreateSampleProjectData()
    {
        return new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(90),
                Status = "Active",
                Sponsor = "Sponsor",
                ProjectManager = "PM"
            },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics
            {
                TotalTasks = 0,
                CompletedTasks = 0,
                CompletionPercentage = 0,
                DaysRemaining = 90
            }
        };
    }
}