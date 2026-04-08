using System;
using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsParameterBindingTests : TestContext
{
    [Fact]
    public void ProgressMetrics_AcceptsProjectMetricsParameter()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20),
            EstimatedBurndownRate = 2.5,
            InProgressTasks = 30,
            CarriedOverTasks = 20
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.NotNull(component.Instance.Metrics);
        Assert.Equal(100, component.Instance.Metrics.TotalTasks);
        Assert.Equal(50, component.Instance.Metrics.CompletedTasks);
    }

    [Fact]
    public void ProgressMetrics_UpdatesWhenMetricsParameterChanges()
    {
        // Arrange
        var initialMetrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, initialMetrics)
        );

        var updatedMetrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 75,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        component.SetParametersAsync(parameters => parameters
            .Add(p => p.Metrics, updatedMetrics)
        );

        // Assert
        Assert.Contains("75%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_RespondsToNullMetricsParameter()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Act
        component.SetParametersAsync(parameters => parameters
            .Add(p => p.Metrics, null)
        );

        // Assert
        Assert.Contains("Metrics unavailable", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_RespectsTotalTasksProperty()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 200,
            CompletedTasks = 100,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("100 of 200 tasks", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_RespectsCompletedTasksProperty()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 50,
            CompletedTasks = 10,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("10 of 50 tasks", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_RespectsProjectStartDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = startDate,
            ProjectEndDate = endDate
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.NotNull(component.Instance.Metrics);
        Assert.Equal(startDate, component.Instance.Metrics.ProjectStartDate);
    }

    [Fact]
    public void ProgressMetrics_RespectsProjectEndDate()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = startDate,
            ProjectEndDate = endDate
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.NotNull(component.Instance.Metrics);
        Assert.Equal(endDate, component.Instance.Metrics.ProjectEndDate);
    }

    [Fact]
    public void ProgressMetrics_RespectsEstimatedBurndownRate()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20),
            EstimatedBurndownRate = 3.5
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.NotNull(component.Instance.Metrics);
        Assert.Equal(3.5, component.Instance.Metrics.EstimatedBurndownRate);
    }
}