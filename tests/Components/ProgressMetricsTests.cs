using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsTests : TestContext
{
    [Fact]
    public void ProgressMetrics_WithNullMetrics_DisplaysZeroValues()
    {
        // Arrange & Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, null)
            .Add(p => p.TotalTasks, 0));

        // Assert
        var content = component.Markup;
        Assert.Contains("0%", content);
    }

    [Fact]
    public void ProgressMetrics_DisplaysCompletionPercentage()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 75,
            TasksShipped = 3,
            TasksInProgress = 1,
            TasksCarriedOver = 0
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 4));

        // Assert
        var content = component.Markup;
        Assert.Contains("75%", content);
    }

    [Fact]
    public void ProgressMetrics_DisplaysProgressBar()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            TasksShipped = 2,
            TasksInProgress = 2,
            TasksCarriedOver = 0
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 4));

        // Assert
        var progressBar = component.Find(".progress-bar");
        Assert.NotNull(progressBar);
        var style = progressBar.GetAttribute("style");
        Assert.Contains("width: 50%", style);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTotalTasks()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 25,
            TasksShipped = 1,
            TasksInProgress = 2,
            TasksCarriedOver = 1
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 4));

        // Assert
        var content = component.Markup;
        Assert.Contains("4", content);
    }

    [Fact]
    public void ProgressMetrics_DisplaysShippedTaskCount()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 60,
            TasksShipped = 6,
            TasksInProgress = 3,
            TasksCarriedOver = 1
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 10));

        // Assert
        var content = component.Markup;
        Assert.Contains("Tasks Shipped", content);
        Assert.Contains("6", content);
    }

    [Fact]
    public void ProgressMetrics_DisplaysInProgressTaskCount()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 40,
            TasksShipped = 4,
            TasksInProgress = 4,
            TasksCarriedOver = 2
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 10));

        // Assert
        var content = component.Markup;
        Assert.Contains("Tasks In Progress", content);
        Assert.Contains("4", content);
    }

    [Fact]
    public void ProgressMetrics_DisplaysCarriedOverTaskCount()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 30,
            TasksShipped = 3,
            TasksInProgress = 2,
            TasksCarriedOver = 5
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 10));

        // Assert
        var content = component.Markup;
        Assert.Contains("Tasks Carried Over", content);
        Assert.Contains("5", content);
    }

    [Fact]
    public void ProgressMetrics_DisplaysAllMetricCards()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            TasksShipped = 5,
            TasksInProgress = 3,
            TasksCarriedOver = 2
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 10));

        // Assert
        var cards = component.FindAll(".metric-card");
        Assert.Equal(5, cards.Count);
    }

    [Fact]
    public void ProgressMetrics_WithZeroCompletion_DisplaysEmptyProgressBar()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 0,
            TasksShipped = 0,
            TasksInProgress = 5,
            TasksCarriedOver = 0
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 5));

        // Assert
        var progressBar = component.Find(".progress-bar");
        var style = progressBar.GetAttribute("style");
        Assert.Contains("width: 0%", style);
    }

    [Fact]
    public void ProgressMetrics_WithFullCompletion_DisplaysFullProgressBar()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 100,
            TasksShipped = 10,
            TasksInProgress = 0,
            TasksCarriedOver = 0
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
            .Add(p => p.TotalTasks, 10));

        // Assert
        var progressBar = component.Find(".progress-bar");
        var style = progressBar.GetAttribute("style");
        Assert.Contains("width: 100%", style);
    }
}