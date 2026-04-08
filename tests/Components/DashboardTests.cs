using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class DashboardTests : TestContext
{
    private readonly MockProjectDataService mockDataService;

    public DashboardTests()
    {
        mockDataService = new MockProjectDataService();
        Services.AddScoped<ProjectDataService>(_ => mockDataService);
        Services.AddLogging();
    }

    [Fact]
    public async Task Dashboard_OnInitialized_LoadsProjectData()
    {
        // Arrange
        var projectData = new ProjectData
        {
            ProjectName = "Test Project",
            ProjectDescription = "Test Description",
            CompletionPercentage = 50,
            Tasks = new List<TaskItem>()
        };
        mockDataService.SetProjectData(projectData);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

        // Assert
        var markup = component.Markup;
        Assert.Contains("Test Project", markup);
        Assert.Contains("Test Description", markup);
    }

    [Fact]
    public async Task Dashboard_WithLoadingState_DisplaysLoadingMessage()
    {
        // Arrange
        mockDataService.SetDelay(TimeSpan.FromSeconds(2));

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert - should show loading initially
        var markup = component.Markup;
        Assert.Contains("Loading dashboard data", markup);
    }

    [Fact]
    public async Task Dashboard_GroupsTasksByStatus()
    {
        // Arrange
        var projectData = new ProjectData
        {
            ProjectName = "Test Project",
            ProjectDescription = "Test",
            CompletionPercentage = 50,
            Tasks = new List<TaskItem>
            {
                new() { Id = "1", Name = "Shipped Task", Owner = "Alice", Status = TaskStatus.Shipped },
                new() { Id = "2", Name = "InProgress Task", Owner = "Bob", Status = TaskStatus.InProgress },
                new() { Id = "3", Name = "CarriedOver Task", Owner = "Charlie", Status = TaskStatus.CarriedOver }
            }
        };
        mockDataService.SetProjectData(projectData);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

        // Assert
        var markup = component.Markup;
        Assert.Contains("Shipped", markup);
        Assert.Contains("In-Progress", markup);
        Assert.Contains("Carried-Over", markup);
    }

    [Fact]
    public async Task Dashboard_WithServiceError_DisplaysErrorMessage()
    {
        // Arrange
        mockDataService.SetError(new InvalidOperationException("Test error"));

        // Act
        var component = RenderComponent<Dashboard>();
        await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

        // Assert
        var markup = component.Markup;
        Assert.Contains("Error Loading Dashboard", markup);
        Assert.Contains("Test error", markup);
    }

    [Fact]
    public async Task Dashboard_DisplaysProjectFooterWithMetrics()
    {
        // Arrange
        var projectData = new ProjectData
        {
            ProjectName = "Test",
            CompletionPercentage = 75,
            Tasks = new List<TaskItem>
            {
                new() { Id = "1", Name = "Task", Owner = "Alice", Status = TaskStatus.Shipped }
            }
        };
        mockDataService.SetProjectData(projectData);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

        // Assert
        var markup = component.Markup;
        Assert.Contains("Total Tasks", markup);
        Assert.Contains("Completion", markup);
        Assert.Contains("1", markup);
        Assert.Contains("75%", markup);
    }

    [Fact]
    public async Task Dashboard_WithNoTasks_DisplaysEmptyState()
    {
        // Arrange
        var projectData = new ProjectData
        {
            ProjectName = "Empty Project",
            CompletionPercentage = 0,
            Tasks = new List<TaskItem>()
        };
        mockDataService.SetProjectData(projectData);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

        // Assert
        var markup = component.Markup;
        Assert.Contains("Empty Project", markup);
    }

    [Fact]
    public async Task Dashboard_RetryButton_ReloadsData()
    {
        // Arrange
        mockDataService.SetError(new InvalidOperationException("Initial error"));
        var component = RenderComponent<Dashboard>();
        await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

        // Act - Set valid data and retry
        var projectData = new ProjectData
        {
            ProjectName = "Recovered Project",
            CompletionPercentage = 50,
            Tasks = new List<TaskItem>()
        };
        mockDataService.SetProjectData(projectData);
        mockDataService.SetError(null);

        var retryButton = component.Find("button.btn-danger");
        retryButton.Click();
        await Task.Delay(100);

        // Assert
        var markup = component.Markup;
        Assert.Contains("Recovered Project", markup);
        Assert.DoesNotContain("Error Loading Dashboard", markup);
    }

    private class MockProjectDataService : ProjectDataService
    {
        private ProjectData? _projectData;
        private Exception? _error;
        private TimeSpan _delay = TimeSpan.Zero;

        public MockProjectDataService() 
            : base(
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ProjectDataService>(),
                new MockWebHostEnvironment())
        {
        }

        public void SetProjectData(ProjectData data) => _projectData = data;
        public void SetError(Exception? error) => _error = error;
        public void SetDelay(TimeSpan delay) => _delay = delay;

        public override async Task<ProjectData?> LoadProjectDataAsync()
        {
            if (_delay > TimeSpan.Zero)
                await Task.Delay(_delay);

            if (_error != null)
                throw _error;

            return _projectData;
        }
    }

    private class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public string WebRootFileProvider { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "Test";
    }
}