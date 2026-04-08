using Bunit;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AgentSquad.Dashboard.Services;
using AgentSquad.Runner.Pages;

namespace AgentSquad.Tests.Components;

public class DashboardComponentTests : TestContext
{
    private readonly Mock<ProjectDataService> _dataServiceMock;
    private readonly Mock<ILogger<Dashboard>> _loggerMock;

    public DashboardComponentTests()
    {
        _dataServiceMock = new Mock<ProjectDataService>(new Mock<ILogger<ProjectDataService>>().Object);
        _loggerMock = new Mock<ILogger<Dashboard>>();
    }

    private ProjectData CreateValidProjectData()
    {
        return new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Q2 Mobile App Release",
                Description = "Mobile app redesign and launch",
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
                    Name = "Design Complete",
                    TargetDate = new DateTime(2024, 2, 15),
                    ActualDate = new DateTime(2024, 2, 14),
                    Status = MilestoneStatus.Completed,
                    CompletionPercentage = 100
                },
                new()
                {
                    Id = "m2",
                    Name = "Development",
                    TargetDate = new DateTime(2024, 4, 30),
                    ActualDate = null,
                    Status = MilestoneStatus.InProgress,
                    CompletionPercentage = 60
                }
            },
            Tasks = new List<Task>
            {
                new()
                {
                    Id = "t1",
                    Name = "API Integration",
                    Status = TaskStatus.Shipped,
                    AssignedTo = "Bob Johnson",
                    DueDate = new DateTime(2024, 3, 1),
                    EstimatedDays = 5,
                    RelatedMilestone = "m1"
                },
                new()
                {
                    Id = "t2",
                    Name = "UI Implementation",
                    Status = TaskStatus.InProgress,
                    AssignedTo = "Carol White",
                    DueDate = new DateTime(2024, 4, 15),
                    EstimatedDays = 10,
                    RelatedMilestone = "m2"
                },
                new()
                {
                    Id = "t3",
                    Name = "Performance Testing",
                    Status = TaskStatus.CarriedOver,
                    AssignedTo = "David Lee",
                    DueDate = new DateTime(2024, 5, 1),
                    EstimatedDays = 7,
                    RelatedMilestone = "m2"
                }
            },
            Metrics = new ProjectMetrics
            {
                TotalTasks = 3,
                CompletedTasks = 1,
                InProgressTasks = 1,
                CarriedOverTasks = 1,
                EstimatedBurndownRate = 1.5
            }
        };
    }

    #region Happy Path Tests

    [Fact]
    public async Task Dashboard_OnInitialized_LoadsProjectData()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains(projectData.Project.Name, component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithValidData_DisplaysProjectName()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Q2 Mobile App Release", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithValidData_DisplaysProjectDescription()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Mobile app redesign and launch", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersMilestoneTimeline()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Project Milestone Timeline", component.Markup);
            Assert.Contains("Design Complete", component.Markup);
            Assert.Contains("Development", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersStatusCards()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Shipped", component.Markup);
            Assert.Contains("In-Progress", component.Markup);
            Assert.Contains("Carried-Over", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithValidData_DisplaysCorrectTaskCounts()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Shipped</h5>", component.Markup);
            Assert.Contains("In-Progress</h5>", component.Markup);
            Assert.Contains("Carried-Over</h5>", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithValidData_RendersProgressMetrics()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Project Progress", component.Markup);
            Assert.Contains("Overall Completion", component.Markup);
        });
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Dashboard_WhenLoadingFails_DisplaysErrorMessage()
    {
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataLoadException("Test error: File not found"));

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Error Loading Dashboard", component.Markup);
            Assert.Contains("Test error: File not found", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithErrorMessage_DisplaysRetryButton()
    {
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataLoadException("Test error"));

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Retry", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WhenRetryClicked_AttemptsReload()
    {
        var projectData = CreateValidProjectData();
        var callCount = 0;

        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1
                    ? Task.FromException<ProjectData>(new DataLoadException("First call fails"))
                    : Task.FromResult(projectData);
            });

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Error Loading Dashboard", component.Markup);
        });

        var retryButton = component.Find("button:contains('Retry')");
        retryButton?.Click();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Q2 Mobile App Release", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithJsonParseError_DisplaysFriendlyMessage()
    {
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataLoadException("Invalid JSON format: Unexpected token '}' at position 42"));

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Error Loading Dashboard", component.Markup);
            Assert.Contains("Invalid JSON format", component.Markup);
        });
    }

    #endregion

    #region Task Filtering Tests

    [Fact]
    public async Task Dashboard_GetTasksByStatus_ReturnsOnlyShippedTasks()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("API Integration", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_GetTasksByStatus_ReturnsOnlyInProgressTasks()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("UI Implementation", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_GetTasksByStatus_ReturnsOnlyCarriedOverTasks()
    {
        var projectData = CreateValidProjectData();
        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Performance Testing", component.Markup);
        });
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Dashboard_WithNoMilestones_StillRenders()
    {
        var projectData = CreateValidProjectData();
        projectData.Milestones.Clear();

        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Project Milestone Timeline", component.Markup);
        });
    }

    [Fact]
    public async Task Dashboard_WithNoTasks_StillRenders()
    {
        var projectData = CreateValidProjectData();
        projectData.Tasks.Clear();
        projectData.Metrics.TotalTasks = 0;
        projectData.Metrics.CompletedTasks = 0;
        projectData.Metrics.InProgressTasks = 0;
        projectData.Metrics.CarriedOverTasks = 0;

        _dataServiceMock.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _dataServiceMock.Object);
        Services.AddScoped(_ => _loggerMock.Object);

        var component = RenderComponent<Dashboard>();

        await Task.Delay(100);
        component.WaitForAssertion(() =>
        {
            Assert.Contains("Shipped", component.Markup);
        });
    }

    #endregion
}