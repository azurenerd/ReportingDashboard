using Bunit;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Components.Pages;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Acceptance;

public class DashboardAcceptanceTests : TestContext, IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _dataFilePath;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public DashboardAcceptanceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _dataFilePath = Path.Combine(_tempDirectory, "data.json");
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.WebRootPath).Returns(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, true);
    }

    [Fact]
    public async Task Dashboard_LoadsValidJsonAndDisplaysProject()
    {
        // Arrange - User Story 1, 2, 3, 4: Load and display project data
        var sampleData = new
        {
            project = new { name = "Q2 Mobile App Release", description = "Executive dashboard for mobile app launch", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new[] {
                new { id = "m1", name = "Design Complete", targetDate = "2024-02-15", status = 2, completionPercentage = 100 },
                new { id = "m2", name = "Development", targetDate = "2024-04-30", status = 1, completionPercentage = 60 },
                new { id = "m3", name = "Testing", targetDate = "2024-05-31", status = 0, completionPercentage = 0 }
            },
            tasks = new[] {
                new { id = "t1", name = "Mobile UI implementation", owner = "Team A", status = 0, dueDate = "2024-02-15" },
                new { id = "t2", name = "Backend API", owner = "Team B", status = 0, dueDate = "2024-03-01" },
                new { id = "t3", name = "Database schema", owner = "Team B", status = 1, dueDate = "2024-03-15" },
                new { id = "t4", name = "Integration testing", owner = "Team C", status = 1, dueDate = "2024-04-15" }
            },
            summary = new { completionPercentage = 50, tasksShipped = 2, tasksInProgress = 2, tasksCarriedOver = 0 }
        };

        await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(sampleData));

        var service = new ProjectDataService(_mockEnvironment.Object);
        Services.AddScoped(_ => service);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert - AC: Dashboard loads data.json on startup
        var markup = component.Markup;
        Assert.Contains("Q2 Mobile App Release", markup);
        Assert.Contains("Executive dashboard for mobile app launch", markup);
    }

    [Fact]
    public async Task Dashboard_DisplaysMilestoneTimelineAtTop()
    {
        // Arrange - User Story 1: View Project Milestones Timeline
        var sampleData = new
        {
            project = new { name = "Project", description = "Test", startDate = "2024-01-01", endDate = "2024-12-31" },
            milestones = new[] {
                new { id = "m1", name = "Design", targetDate = "2024-03-01", status = 2, completionPercentage = 100 },
                new { id = "m2", name = "Development", targetDate = "2024-06-01", status = 1, completionPercentage = 50 },
                new { id = "m3", name = "Launch", targetDate = "2024-09-01", status = 0, completionPercentage = 0 }
            },
            tasks = new object[] { },
            summary = new { completionPercentage = 0, tasksShipped = 0, tasksInProgress = 0, tasksCarriedOver = 0 }
        };

        await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(sampleData));

        var service = new ProjectDataService(_mockEnvironment.Object);
        Services.AddScoped(_ => service);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert - AC: Timeline displays milestone name, target date, and completion status
        var markup = component.Markup;
        Assert.Contains("Milestones", markup);
        Assert.Contains("Design", markup);
        Assert.Contains("Development", markup);
        Assert.Contains("Launch", markup);
    }

    [Fact]
    public async Task Dashboard_DisplaysTaskStatusCards()
    {
        // Arrange - User Story 2: Monitor Task Status Breakdown
        var sampleData = new
        {
            project = new { name = "Project", description = "Test", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new object[] { },
            tasks = new[] {
                new { id = "t1", name = "Feature A", owner = "Alice", status = 0, dueDate = "2024-02-01" },
                new { id = "t2", name = "Feature B", owner = "Bob", status = 0, dueDate = "2024-02-15" },
                new { id = "t3", name = "Feature C", owner = "Charlie", status = 1, dueDate = "2024-03-01" },
                new { id = "t4", name = "Feature D", owner = "Diana", status = 2, dueDate = "2024-03-15" }
            },
            summary = new { completionPercentage = 25, tasksShipped = 1, tasksInProgress = 1, tasksCarriedOver = 2 }
        };

        await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(sampleData));

        var service = new ProjectDataService(_mockEnvironment.Object);
        Services.AddScoped(_ => service);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert - AC: Three status cards display with counts and color coding
        var markup = component.Markup;
        Assert.Contains("Task Status", markup);
        Assert.Contains("Shipped", markup);
        Assert.Contains("In Progress", markup);
        Assert.Contains("Carried Over", markup);
    }

    [Fact]
    public async Task Dashboard_DisplaysProgressMetrics()
    {
        // Arrange - User Story 3: View Progress Metrics
        var sampleData = new
        {
            project = new { name = "Project", description = "Test", startDate = "2024-01-01", endDate = "2024-06-30" },
            milestones = new object[] { },
            tasks = new[] {
                new { id = "t1", name = "Task 1", owner = "Alice", status = 0, dueDate = "2024-02-01" },
                new { id = "t2", name = "Task 2", owner = "Bob", status = 0, dueDate = "2024-02-15" },
                new { id = "t3", name = "Task 3", owner = "Charlie", status = 1, dueDate = "2024-03-01" },
                new { id = "t4", name = "Task 4", owner = "Diana", status = 1, dueDate = "2024-03-15" }
            },
            summary = new { completionPercentage = 50, tasksShipped = 2, tasksInProgress = 2, tasksCarriedOver = 0 }
        };

        await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(sampleData));

        var service = new ProjectDataService(_mockEnvironment.Object);
        Services.AddScoped(_ => service);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert - AC: Progress chart displays completion percentage and is responsive
        var markup = component.Markup;
        Assert.Contains("Progress Metrics", markup);
        Assert.Contains("Overall Completion", markup);
        Assert.Contains("50%", markup);
    }

    [Fact]
    public async Task Dashboard_DisplaysErrorForMalformedJson()
    {
        // Arrange - User Story 4: AC: Malformed JSON displays user-friendly error message
        await File.WriteAllTextAsync(_dataFilePath, "{ invalid json");

        var service = new ProjectDataService(_mockEnvironment.Object);
        Services.AddScoped(_ => service);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var markup = component.Markup;
        Assert.Contains("Error Loading Data", markup);
        Assert.Contains("Invalid JSON", markup);
    }

    [Fact]
    public async Task Dashboard_DisplaysErrorForMissingFile()
    {
        // Arrange - User Story 4: AC: File not found displays error
        var service = new ProjectDataService(_mockEnvironment.Object);
        Services.AddScoped(_ => service);
        Services.AddScoped(_ => _mockEnvironment.Object);

        // Act
        var component = RenderComponent<Dashboard>();
        await component.Instance.InvokeAsync(() => { });

        // Assert
        var markup = component.Markup;
        Assert.Contains("Error Loading Data", markup);
        Assert.Contains("not found", markup);
    }
}