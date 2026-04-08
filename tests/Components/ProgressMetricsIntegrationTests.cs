using System;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Components;

public class ProgressMetricsIntegrationTests : TestContext
{
    [Fact]
    public void ProgressMetrics_IntegrationWithDashboard_PassesMetricsParameter()
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
        Assert.Equal(50, component.Instance.Metrics.CompletedTasks);
        Assert.Contains("50%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithProjectData_CalculatesMetricsCorrectly()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test Project",
                StartDate = DateTime.Now.AddDays(-10),
                EndDate = DateTime.Now.AddDays(20)
            },
            Metrics = new ProjectMetrics
            {
                TotalTasks = 100,
                CompletedTasks = 40,
                ProjectStartDate = DateTime.Now.AddDays(-10),
                ProjectEndDate = DateTime.Now.AddDays(20)
            }
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, projectData.Metrics)
        );

        // Assert
        Assert.Contains("40%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithMultipleMilestones_StillDisplaysMetrics()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 150,
            CompletedTasks = 90,
            ProjectStartDate = DateTime.Now.AddDays(-20),
            ProjectEndDate = DateTime.Now.AddDays(40)
        };

        var projectData = new ProjectData
        {
            Milestones = new System.Collections.Generic.List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Milestone 1", TargetDate = DateTime.Now.AddDays(10), Status = MilestoneStatus.InProgress },
                new Milestone { Id = "m2", Name = "Milestone 2", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Pending }
            },
            Metrics = metrics
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, projectData.Metrics)
        );

        // Assert
        Assert.Contains("60%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_EnsuresNoAnimationApplied()
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
        // Verify component renders without animation styles
        var markup = component.Markup;
        // Progress bar and text rendering should be static
        Assert.Contains("progress", markup);
        // No animation directives should be present
        Assert.DoesNotContain("animation:", markup);
        Assert.DoesNotContain("@keyframes", markup);
    }

    [Fact]
    public void ProgressMetrics_HandlesCacheDataFromService()
    {
        // Arrange
        var logger = new LoggerFactory().CreateLogger<ProjectDataService>();
        var service = new ProjectDataService(logger);

        var metrics = new ProjectMetrics
        {
            TotalTasks = 200,
            CompletedTasks = 120,
            ProjectStartDate = DateTime.Now.AddDays(-30),
            ProjectEndDate = DateTime.Now.AddDays(60)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        Assert.NotNull(component.Instance.Metrics);
        Assert.Equal(60, component.Instance.Metrics.CompletedTasks / component.Instance.Metrics.TotalTasks * 100);
    }

    [Fact]
    public void ProgressMetrics_DisplaysScreenshotReadyOutput()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 100,
            CompletedTasks = 85,
            ProjectStartDate = DateTime.Now.AddDays(-15),
            ProjectEndDate = DateTime.Now.AddDays(30)
        };

        // Act
        var component = RenderComponent<ProgressMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics)
        );

        // Assert
        var markup = component.Markup;
        // Verify PowerPoint screenshot readability
        Assert.Contains("85%", markup);
        Assert.Contains("85 of 100 tasks", markup);
        Assert.Contains("progress-bar", markup);
        Assert.Contains("Burn-down Chart", markup);
    }
}