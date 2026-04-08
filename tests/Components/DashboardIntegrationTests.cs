using Bunit;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Components.Pages;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class DashboardIntegrationTests : TestContext
{
    private readonly Mock<ProjectDataService> _mockDataService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public DashboardIntegrationTests()
    {
        _mockDataService = new Mock<ProjectDataService>(new MockWebHostEnvironment());
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());
    }

    [Fact]
    public async Task Dashboard_OnInitialized_LoadsProjectData()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Q2 Release", Description = "Mobile app launch", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6) },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Summary = new ProjectMetrics { CompletionPercentage = 0, TasksShipped = 0, TasksInProgress = 0, TasksCarriedOver = 0 }
        };

        _mockDataService.Setup(x => x.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _mockDataService.Object);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        _mockDataService.Verify(x => x.LoadProjectDataAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Dashboard_DisplaysProjectName()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Q2 Release", Description = "Mobile app launch", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6) },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Summary = new ProjectMetrics()
        };

        _mockDataService.Setup(x => x.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _mockDataService.Object);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var content = component.Markup;
        Assert.Contains("Q2 Release", content);
    }

    [Fact]
    public async Task Dashboard_WithLoadError_DisplaysErrorAlert()
    {
        // Arrange
        _mockDataService.Setup(x => x.LoadProjectDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new DataLoadException("Invalid JSON format"));

        Services.AddScoped(_ => _mockDataService.Object);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var content = component.Markup;
        Assert.Contains("Error Loading Data", content);
        Assert.Contains("Invalid JSON format", content);
    }

    [Fact]
    public async Task Dashboard_DisplaysTaskStatusCards()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Test", Description = "Test", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6) },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>
            {
                new() { Id = "t1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped, DueDate = DateTime.Now },
                new() { Id = "t2", Name = "Task 2", Owner = "Bob", Status = TaskStatus.InProgress, DueDate = DateTime.Now }
            },
            Summary = new ProjectMetrics { CompletionPercentage = 50, TasksShipped = 1, TasksInProgress = 1, TasksCarriedOver = 0 }
        };

        _mockDataService.Setup(x => x.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _mockDataService.Object);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var content = component.Markup;
        Assert.Contains("Task Status", content);
    }

    [Fact]
    public async Task Dashboard_DisplaysMilestoneTimeline()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Test", Description = "Test", StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 6, 30) },
            Milestones = new List<Milestone>
            {
                new() { Id = "m1", Name = "Alpha", TargetDate = new DateTime(2024, 2, 1), Status = MilestoneStatus.Completed, CompletionPercentage = 100 }
            },
            Tasks = new List<Task>(),
            Summary = new ProjectMetrics()
        };

        _mockDataService.Setup(x => x.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _mockDataService.Object);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var content = component.Markup;
        Assert.Contains("Milestones", content);
    }

    [Fact]
    public async Task Dashboard_DisplaysProgressMetrics()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo { Name = "Test", Description = "Test", StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(6) },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Summary = new ProjectMetrics { CompletionPercentage = 75, TasksShipped = 3, TasksInProgress = 1, TasksCarriedOver = 0 }
        };

        _mockDataService.Setup(x => x.LoadProjectDataAsync(It.IsAny<string>()))
            .ReturnsAsync(projectData);

        Services.AddScoped(_ => _mockDataService.Object);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var content = component.Markup;
        Assert.Contains("Progress Metrics", content);
    }

    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Test";
    }
}