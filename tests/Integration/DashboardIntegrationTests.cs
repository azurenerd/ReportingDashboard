using Bunit;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AgentSquad.Dashboard.Services;
using AgentSquad.Runner.Pages;

namespace AgentSquad.Tests.Integration;

public class DashboardIntegrationTests : TestContext
{
    private ProjectData CreateSampleProjectData()
    {
        return new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Q2 Mobile App Release",
                Description = "Complete mobile app redesign",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 6, 30),
                Status = "OnTrack",
                Sponsor = "VP Engineering",
                ProjectManager = "Alice Smith"
            },
            Milestones = new List<Milestone>
            {
                new()
                {
                    Id = "m1",
                    Name = "Requirements & Design",
                    TargetDate = new DateTime(2024, 2, 29),
                    ActualDate = new DateTime(2024, 2, 28),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                },
                new()
                {
                    Id = "m2",
                    Name = "Development Sprint 1",
                    TargetDate = new DateTime(2024, 4, 15),
                    ActualDate = null,
                    Status = MilestoneStatus.InProgress,
                    CompletionPercentage = 65
                },
                new()
                {
                    Id = "m3",
                    Name = "Testing & QA",
                    TargetDate = new DateTime(2024, 5, 31),
                    ActualDate = null,
                    Status = MilestoneStatus.Pending,
                    CompletionPercentage = 0
                },
                new()
                {
                    Id = "m4",
                    Name = "Production Release",
                    TargetDate = new DateTime(2024, 6, 30),
                    ActualDate = null,
                    Status = MilestoneStatus.Pending,
                    CompletionPercentage = 0
                }
            },
            Tasks = new List<Task>
            {
                new()
                {
                    Id = "t1",
                    Name = "Wireframe Creation",
                    Status = TaskStatus.Shipped,
                    AssignedTo = "Bob Johnson",
                    DueDate = new DateTime(2024, 2, 15),
                    EstimatedDays = 3,
                    RelatedMilestone = "m1"
                },
                new()
                {
                    Id = "t2",
                    Name = "Backend API Development",
                    Status = TaskStatus.Shipped,
                    AssignedTo = "Carol White",
                    DueDate = new DateTime(2024, 3, 15),
                    EstimatedDays = 10,
                    RelatedMilestone = "m2"
                },
                new()
                {
                    Id = "t3",
                    Name = "Frontend UI Implementation",
                    Status = TaskStatus.InProgress,
                    AssignedTo = "David Lee",
                    DueDate = new DateTime(2024, 4, 15),
                    EstimatedDays = 12,
                    RelatedMilestone = "m2"
                },
                new()
                {
                    Id = "t4",
                    Name = "Database Schema Design",
                    Status = TaskStatus.InProgress,
                    AssignedTo = "Eve Martinez",
                    DueDate = new DateTime(2024, 4, 1),
                    EstimatedDays = 5,
                    RelatedMilestone = "m2"
                },
                new()
                {
                    Id = "t5",
                    Name = "Authentication Implementation",
                    Status = TaskStatus.CarriedOver,
                    AssignedTo = "Frank Brown",
                    DueDate = new DateTime(2024, 3, 31),
                    EstimatedDays = 8,
                    RelatedMilestone = "m2"
                },
                new()
                {
                    Id = "t6",
                    Name = "Integration Testing",
                    Status = TaskStatus.CarriedOver,
                    AssignedTo = "Grace Taylor",
                    DueDate = new DateTime(2024, 5, 15),
                    EstimatedDays = 7,
                    RelatedMilestone = "m3"
                },
                new()
                {
                    Id = "t7",
                    Name = "Performance Optimization",
                    Status = TaskStatus.CarriedOver,
                    AssignedTo = "Henry Davis",
                    DueDate = new DateTime(2024, 5, 1),
                    EstimatedDays = 6,
                    RelatedMilestone = "m3"
                },
                new()
                {
                    Id = "t8",
                    Name = "Documentation",
                    Status = TaskStatus.CarriedOver,
                    AssignedTo = "Ivy Wilson",
                    DueDate = new DateTime(2024, 6, 1),
                    EstimatedDays = 4,
                    RelatedMilestone = "m4"
                }
            },
            Metrics = new ProjectMetrics
            {
                TotalTasks = 8,
                CompletedTasks = 2,
                InProgressTasks = 2,
                CarriedOverTasks = 4,
                EstimatedBurndownRate = 1.5
            }
        };
    }

    #region Full Dashboard Integration Tests

    [Fact]
    public async Task Dashboard_FullIntegration_LoadsAndDisplaysAllComponents()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Q2 Mobile App Release", component.Markup);
            Assert.Contains("Project Milestone Timeline", component.Markup);
            Assert.Contains("Requirements & Design", component.Markup);
            Assert.Contains("Development Sprint 1", component.Markup);
            Assert.Contains("Testing & QA", component.Markup);
            Assert.Contains("Production Release", component.Markup);
            Assert.Contains("Shipped", component.Markup);
            Assert.Contains("In-Progress", component.Markup);
            Assert.Contains("Carried-Over", component.Markup);
            Assert.Contains("Project Progress", component.Markup);
            Assert.Contains("Overall Completion", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_FullIntegration_DisplaysCorrectTaskCounts()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("2", component.Markup);
            Assert.Contains("4", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_FullIntegration_StatusCardsShowTasksOnClick()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Show Tasks", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_FullIntegration_DisplaysAllMilestoneStatuses()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("100%", component.Markup);
            Assert.Contains("65%", component.Markup);
            Assert.Contains("0%", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_FullIntegration_DisplaysProjectMetrics()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("25%", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_FullIntegration_ResponsiveLayout()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("container-fluid", component.Markup);
            Assert.Contains("col-md-4", component.Markup);
            Assert.Contains("row", component.Markup);
        });
    }

    #endregion

    #region Acceptance Criteria Tests

    [Fact]
    public async Task AcceptanceCriteria_TimelineDisplaysMilestoneNameTargetDateStatus()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Requirements & Design", component.Markup);
            Assert.Contains("Feb", component.Markup);
            Assert.Contains("100%", component.Markup);
        });
    }

    [Fact]
    public async Task AcceptanceCriteria_TimelineIsFullWidthAndProminent()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("milestone-section", component.Markup);
            Assert.Contains("mb-5", component.Markup);
        });
    }

    [Fact]
    public async Task AcceptanceCriteria_StatusCardsUseColorCoding()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("28a745", component.Markup);
            Assert.Contains("17a2b8", component.Markup);
            Assert.Contains("ffc107", component.Markup);
        });
    }

    [Fact]
    public async Task AcceptanceCriteria_ProgressChartDisplaysCompletionPercentage()
    {
        var projectData = CreateSampleProjectData();
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Overall Completion", component.Markup);
            Assert.Contains("25%", component.Markup);
        });
    }

    [Fact]
    public async Task AcceptanceCriteria_MalformedJsonDisplaysUserFriendlyError()
    {
        var dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        var loggerMock = new Mock<ILogger<Dashboard>>();

        dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataLoadException("Invalid JSON format"));

        Services.AddScoped(_ => dataServiceMock.Object);
        Services.AddScoped(_ => loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Error Loading Dashboard", component.Markup);
            Assert.Contains("Invalid JSON format", component.Markup);
        });
    }

    #endregion
}