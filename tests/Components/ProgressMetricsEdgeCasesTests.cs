using System;
using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsEdgeCasesTests : TestContext
{
    [Fact]
    public void ProgressMetrics_WithOne100PercentCompletion_DisplaysCorrectly()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 1,
            CompletedTasks = 1,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(5)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("100%", component.Markup);
        var progressBar = component.Find(".progress-bar");
        var style = progressBar.GetAttribute("style");
        Assert.Contains("width: 100%", style);
    }

    [Fact]
    public void ProgressMetrics_WithOne0PercentCompletion_DisplaysCorrectly()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 1,
            CompletedTasks = 0,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(5)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("0%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithVeryLargeDates_HandlesCorrectly()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 1000,
            CompletedTasks = 500,
            ProjectStartDate = new DateTime(2000, 1, 1),
            ProjectEndDate = new DateTime(2050, 12, 31)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("50%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithFractionalCompletion_RoundsCorrectly()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 3,
            CompletedTasks = 1,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(5)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("33%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithSameDateStartAndEnd_IsInvalid()
    {
        // Arrange
        var sameDate = DateTime.Now;
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = sameDate,
            ProjectEndDate = sameDate
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("Invalid metrics data", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_PartiallyNullMetrics_DisplaysWarning()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = default
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("Invalid metrics data", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithVerySmallProject_DisplaysCorrectly()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 1,
            CompletedTasks = 0,
            ProjectStartDate = DateTime.Now,
            ProjectEndDate = DateTime.Now.AddSeconds(1)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("0%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithExtremelylLargeProjectFile_HandlesGracefully()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10000,
            CompletedTasks = 9999,
            ProjectStartDate = DateTime.Now.AddDays(-365),
            ProjectEndDate = DateTime.Now.AddDays(1)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var markup = component.Markup;
        Assert.NotEmpty(markup);
        Assert.Contains("progress", markup);
    }
}