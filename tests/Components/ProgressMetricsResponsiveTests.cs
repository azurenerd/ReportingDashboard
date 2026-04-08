using System;
using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsResponsiveTests : TestContext
{
    [Fact]
    public void ProgressMetrics_ContainerUsesBootstrapGrid()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("row", component.Markup);
        Assert.Contains("col-12", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_ResponsiveContainerClass()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var container = component.Find(".progress-metrics-container");
        Assert.NotNull(container);
    }

    [Fact]
    public void ProgressMetrics_HeadingUsesForecastStyleForReadability()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var heading = component.Find("h3");
        var classes = heading.GetAttribute("class");
        Assert.Contains("text-muted", classes);
        Assert.Contains("mb-3", classes);
    }

    [Fact]
    public void ProgressMetrics_UsesBootstrapMarginClasses()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("mb-4", component.Markup);
        Assert.Contains("mb-3", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_TextCenteredForCompletion()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var completionDiv = component.Find(".completion-percentage");
        var classes = completionDiv.GetAttribute("class");
        Assert.Contains("text-center", classes);
    }

    [Fact]
    public void ProgressMetrics_BurndownSectionDisplay()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("burndown-section", component.Markup);
    }
}