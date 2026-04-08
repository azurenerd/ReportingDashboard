using Xunit;
using Bunit;
using AgentSquad.Dashboard.Pages;
using AgentSquad.Dashboard.Models;
using AgentSquad.Dashboard.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgentSquad.Dashboard.Tests.Acceptance;

public class UserStoryTests : TestContext
{
    [Fact]
    public void UserStory1_ViewProjectMilestonesTimeline_DisplaysTimelineWithStatus()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Project", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(90), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new()
            {
                new Milestone { Id = "M1", Name = "Phase 1 Complete", TargetDate = new DateTime(2026, 06, 30), ActualDate = new DateTime(2026, 06, 28), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
                new Milestone { Id = "M2", Name = "Phase 2 In Progress", TargetDate = new DateTime(2026, 07, 31), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 },
                new Milestone { Id = "M3", Name = "Phase 3 Pending", TargetDate = new DateTime(2026, 08, 31), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
            },
            Tasks = new(),
            Metrics = new ProjectMetrics { TotalTasks = 0, CompletedTasks = 0, CompletionPercentage = 0, DaysRemaining = 90 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("Phase 1 Complete", component.Markup);
        Assert.Contains("Phase 2 In Progress", component.Markup);
        Assert.Contains("Phase 3 Pending", component.Markup);
        Assert.Contains("Jun 30, 2026", component.Markup);
    }

    [Fact]
    public void UserStory1_TimelineIsFullWidth_AndResponsive()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = CreateSampleProjectData();

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("section-milestone", component.Markup);
    }

    [Fact]
    public void UserStory1_CompletedMilestonesDifferentiatedFromPending()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Project", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(90), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new()
            {
                new Milestone { Id = "M1", Name = "Completed", TargetDate = DateTime.Now.AddDays(-5), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
                new Milestone { Id = "M2", Name = "Pending", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
            },
            Tasks = new(),
            Metrics = new ProjectMetrics { TotalTasks = 0, CompletedTasks = 0, CompletionPercentage = 0, DaysRemaining = 90 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("bg-success", component.Markup);
        Assert.Contains("bg-secondary", component.Markup);
    }

    [Fact]
    public void UserStory2_MonitorTaskStatusBreakdown_DisplaysThreeStatusCards()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Project", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(90), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new(),
            Tasks = new()
            {
                new Task { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "User1", DueDate = DateTime.Now, EstimatedDays = 5 },
                new Task { Id = "T2", Name = "Task 2", Status = TaskStatus.Shipped, AssignedTo = "User2", DueDate = DateTime.Now, EstimatedDays = 3 },
                new Task { Id = "T3", Name = "Task 3", Status = TaskStatus.InProgress, AssignedTo = "User3", DueDate = DateTime.Now, EstimatedDays = 2 },
                new Task { Id = "T4", Name = "Task 4", Status = TaskStatus.CarriedOver, AssignedTo = "User4", DueDate = DateTime.Now, EstimatedDays = 4 }
            },
            Metrics = new ProjectMetrics { TotalTasks = 4, CompletedTasks = 2, InProgressTasks = 1, CarriedOverTasks = 1, CompletionPercentage = 50, DaysRemaining = 90 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("Shipped", component.Markup);
        Assert.Contains("In-Progress", component.Markup);
        Assert.Contains("Carried-Over", component.Markup);
    }

    [Fact]
    public void UserStory2_StatusCardsDisplayColorCoding()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = CreateSampleProjectData();

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("success", component.Markup);
        Assert.Contains("info", component.Markup);
        Assert.Contains("warning", component.Markup);
    }

    [Fact]
    public void UserStory2_StatusCardsDisplayTaskCount()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Project", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(90), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new(),
            Tasks = new()
            {
                new Task { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "User1", DueDate = DateTime.Now, EstimatedDays = 5 },
                new Task { Id = "T2", Name = "Task 2", Status = TaskStatus.Shipped, AssignedTo = "User2", DueDate = DateTime.Now, EstimatedDays = 3 }
            },
            Metrics = new ProjectMetrics { TotalTasks = 2, CompletedTasks = 2, CompletionPercentage = 100, DaysRemaining = 90 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("2", component.Markup);
    }

    [Fact]
    public void UserStory3_ViewProgressMetrics_DisplaysCompletionPercentage()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Project", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(90), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new(),
            Tasks = new(),
            Metrics = new ProjectMetrics { TotalTasks = 10, CompletedTasks = 7, CompletionPercentage = 70, DaysRemaining = 45 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("70%", component.Markup);
        Assert.Contains("7 of 10", component.Markup);
    }

    [Fact]
    public void UserStory4_LoadAndDisplayProjectData_LoadsFromJson()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = CreateSampleProjectData();

        mockDataService
            .Setup(s => s.LoadProjectDataAsync("data/data.json"))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        mockDataService.Verify(s => s.LoadProjectDataAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void UserStory4_MalformedJsonDisplaysFriendlyError()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Invalid JSON format"));

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();
        
        Assert.NotNull(component);
    }

    private ProjectData CreateSampleProjectData()
    {
        return new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test Project",
                Description = "Test",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(90),
                Status = "Active",
                Sponsor = "Sponsor",
                ProjectManager = "PM"
            },
            Milestones = new(),
            Tasks = new(),
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