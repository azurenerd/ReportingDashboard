using System;
using System.Collections.Generic;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsTests : TestContext
{
    private const string ProgressMetricsComponentPath = "AgentSquad.Runner.Components.ProgressMetrics";

    [Fact]
    public void ProgressMetrics_WithNullMetrics_DisplaysUnavailableMessage()
    {
        // Arrange
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, null)
        );

        // Act & Assert
        var alertDiv = component.Find(".alert-warning");
        Assert.NotNull(alertDiv);
        Assert.Contains("Metrics unavailable", component.Markup);
        Assert.Contains("Project metrics data is not available", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithValidMetrics_DisplaysProgressBar()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20),
            EstimatedBurndownRate = 2.5
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var progressBar = component.Find(".progress-bar");
        Assert.NotNull(progressBar);
        Assert.Contains("50%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysCompletionPercentageAt24ptFont()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 4,
            CompletedTasks = 1,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(15)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var completionDiv = component.Find(".display-4");
        Assert.NotNull(completionDiv);
        var style = completionDiv.GetAttribute("style");
        Assert.Contains("24pt", style);
        Assert.Contains("25%", component.Markup);
    }

    [Theory]
    [InlineData(100, 100, 100)]
    [InlineData(100, 50, 50)]
    [InlineData(100, 0, 0)]
    [InlineData(10, 1, 10)]
    [InlineData(3, 1, 33)]
    public void ProgressMetrics_CalculatesCompletionPercentageCorrectly(int total, int completed, int expected)
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = total,
            CompletedTasks = completed,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(15)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains($"{expected}%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithNegativeTotalTasks_DisplaysInvalidDataWarning()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = -5,
            CompletedTasks = 2,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(15)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var alertDiv = component.Find(".alert-warning");
        Assert.NotNull(alertDiv);
        Assert.Contains("Invalid metrics data", component.Markup);
        Assert.Contains("TotalTasks cannot be negative", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithNegativeCompletedTasks_DisplaysInvalidDataWarning()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = -10,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(15)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("Invalid metrics data", component.Markup);
        Assert.Contains("CompletedTasks cannot be negative", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithCompletedExceedingTotal_DisplaysInvalidDataWarning()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 50,
            CompletedTasks = 100,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(15)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("Invalid metrics data", component.Markup);
        Assert.Contains("CompletedTasks cannot exceed TotalTasks", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithEndDateBeforeStartDate_DisplaysInvalidDataWarning()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 50,
            ProjectStartDate = DateTime.Now.AddDays(10),
            ProjectEndDate = DateTime.Now.AddDays(-5)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("Invalid metrics data", component.Markup);
        Assert.Contains("ProjectEndDate must be after ProjectStartDate", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithZeroTasks_DisplaysNoTasksMessage()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 0,
            CompletedTasks = 0,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = DateTime.Now.AddDays(15)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("No tasks defined", component.Markup);
        Assert.Contains("Project has no tasks configured", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTaskCountSummary()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 35,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.Contains("35 of 100 tasks", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysBurndownSectionWhenMetricsValid()
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
        Assert.Contains("Burn-down Chart", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_ProgressBarWidthReflectsCompletion()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 75,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var progressBar = component.Find(".progress-bar");
        var styleAttr = progressBar.GetAttribute("style");
        Assert.Contains("width: 75%", styleAttr);
    }

    [Fact]
    public void ProgressMetrics_ContainsBg_successClass()
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
        var progressBar = component.Find(".progress-bar");
        Assert.NotNull(progressBar);
        var classes = progressBar.GetAttribute("class");
        Assert.Contains("bg-success", classes);
    }

    [Fact]
    public void ProgressMetrics_ProgressBarHasAccessibilityAttributes()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 60,
            ProjectStartDate = DateTime.Now.AddDays(-10),
            ProjectEndDate = DateTime.Now.AddDays(20)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var progressBar = component.Find(".progress-bar");
        Assert.Equal("progressbar", progressBar.GetAttribute("role"));
        Assert.Equal("60", progressBar.GetAttribute("aria-valuenow"));
        Assert.Equal("0", progressBar.GetAttribute("aria-valuemin"));
        Assert.Equal("100", progressBar.GetAttribute("aria-valuemax"));
    }

    [Fact]
    public void ProgressMetrics_Uses24ptFontForCompletion()
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
        var display = component.Find(".display-4");
        var style = display.GetAttribute("style");
        Assert.NotNull(style);
        Assert.Contains("font-size", style);
        Assert.Contains("24pt", style);
    }
}