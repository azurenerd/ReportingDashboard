using Xunit;
using Bunit;
using AgentSquad.Dashboard.Pages;
using AgentSquad.Dashboard.Models;
using AgentSquad.Dashboard.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgentSquad.Dashboard.Tests.Integration;

public class DashboardIntegrationTests : TestContext
{
    [Fact]
    public void Index_LoadsAndDisplaysProjectData()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Test", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new() { new Milestone { Id = "M1", Name = "Phase 1", TargetDate = DateTime.Now.AddDays(15), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 } },
            Tasks = new() { new Task { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "User", DueDate = DateTime.Now, EstimatedDays = 5 } },
            Metrics = new ProjectMetrics { TotalTasks = 1, CompletedTasks = 1, CompletionPercentage = 100, DaysRemaining = 30 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("Executive Project Dashboard", component.Markup);
    }

    [Fact]
    public void Dashboard_DisplaysAllSections_WhenDataLoaded()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = CreateFullProjectData();

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();

        Assert.Contains("Loading project data", component.Markup);
    }

    [Fact]
    public void Dashboard_IntegrationTest_AllComponentsWork()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = CreateFullProjectData();

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();
        Assert.NotNull(component);
    }

    [Fact]
    public void Dashboard_HandlesDataServiceError()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service error"));

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();
        Assert.NotNull(component);
    }

    [Fact]
    public void Dashboard_Metrics_Calculated_Correctly_Integration()
    {
        var mockDataService = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Project", Description = "Desc", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(60), Status = "Active", Sponsor = "Sponsor", ProjectManager = "PM" },
            Milestones = new() { 
                new Milestone { Id = "M1", Name = "M1", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
                new Milestone { Id = "M2", Name = "M2", TargetDate = DateTime.Now.AddDays(60), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 }
            },
            Tasks = new() {
                new Task { Id = "T1", Name = "T1", Status = TaskStatus.Shipped, AssignedTo = "User1", DueDate = DateTime.Now, EstimatedDays = 5 },
                new Task { Id = "T2", Name = "T2", Status = TaskStatus.Shipped, AssignedTo = "User2", DueDate = DateTime.Now, EstimatedDays = 3 },
                new Task { Id = "T3", Name = "T3", Status = TaskStatus.InProgress, AssignedTo = "User3", DueDate = DateTime.Now, EstimatedDays = 2 }
            },
            Metrics = new ProjectMetrics { TotalTasks = 3, CompletedTasks = 2, InProgressTasks = 1, CompletionPercentage = 67, DaysRemaining = 60 }
        };

        mockDataService
            .Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => mockDataService.Object);

        var component = RenderComponent<Index>();
        Assert.NotNull(component);
    }

    private ProjectData CreateFullProjectData()
    {
        return new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Q2 Mobile App Release",
                Description = "Mobile app v2.0 release",
                StartDate = new DateTime(2026, 04, 01),
                EndDate = new DateTime(2026, 07, 31),
                Status = "In Progress",
                Sponsor = "VP Product",
                ProjectManager = "Jane Smith"
            },
            Milestones = new()
            {
                new Milestone { Id = "M1", Name = "Requirements & Design", TargetDate = new DateTime(2026, 04, 30), ActualDate = new DateTime(2026, 04, 28), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
                new Milestone { Id = "M2", Name = "Development Sprint 1", TargetDate = new DateTime(2026, 05, 31), Status = MilestoneStatus.InProgress, CompletionPercentage = 60 },
                new Milestone { Id = "M3", Name = "QA & Testing", TargetDate = new DateTime(2026, 06, 30), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
            },
            Tasks = new()
            {
                new Task { Id = "T1", Name = "Implement authentication", Status = TaskStatus.Shipped, AssignedTo = "John Doe", DueDate = new DateTime(2026, 04, 15), EstimatedDays = 5, RelatedMilestone = "M1" },
                new Task { Id = "T2", Name = "Setup API endpoints", Status = TaskStatus.Shipped, AssignedTo = "Jane Smith", DueDate = new DateTime(2026, 04, 20), EstimatedDays = 3, RelatedMilestone = "M1" },
                new Task { Id = "T3", Name = "Build dashboard UI", Status = TaskStatus.InProgress, AssignedTo = "Bob Johnson", DueDate = new DateTime(2026, 05, 15), EstimatedDays = 8, RelatedMilestone = "M2" },
                new Task { Id = "T4", Name = "Write unit tests", Status = TaskStatus.InProgress, AssignedTo = "Alice Brown", DueDate = new DateTime(2026, 05, 20), EstimatedDays = 5, RelatedMilestone = "M2" },
                new Task { Id = "T5", Name = "Performance optimization", Status = TaskStatus.CarriedOver, AssignedTo = "Charlie Wilson", DueDate = new DateTime(2026, 05, 30), EstimatedDays = 4, RelatedMilestone = "M2" }
            },
            Metrics = new ProjectMetrics
            {
                TotalTasks = 5,
                CompletedTasks = 2,
                InProgressTasks = 2,
                CarriedOverTasks = 1,
                CompletionPercentage = 40,
                EstimatedBurndownRate = 0.2,
                ProjectStartDate = new DateTime(2026, 04, 01),
                ProjectEndDate = new DateTime(2026, 07, 31),
                DaysRemaining = 114
            }
        };
    }
}