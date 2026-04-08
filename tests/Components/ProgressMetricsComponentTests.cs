using Bunit;
using Xunit;
using AgentSquad.Dashboard.Services;
using AgentSquad.Runner.Components;

namespace AgentSquad.Tests.Components;

public class ProgressMetricsComponentTests : TestContext
{
    #region Happy Path Tests

    [Fact]
    public void ProgressMetrics_DisplaysOverallCompletionSection()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("Overall Completion", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysCompletionPercentage()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("50%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTaskCounts()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("5 of 10 tasks completed", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTimeRemaining()
    {
        var endDate = DateTime.Now.AddDays(30);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, DateTime.Now.AddDays(-30))
                .Add(p => p.ProjectEndDate, endDate)
        );

        Assert.Contains("days", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTargetCompletionDate()
    {
        var endDate = new DateTime(2024, 6, 30);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, endDate)
        );

        Assert.Contains("Jun 30, 2024", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithCompleteProject_Shows100Percent()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 10,
            InProgressTasks = 0,
            CarriedOverTasks = 0,
            EstimatedBurndownRate = 2.0
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("100%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithNoTasks_Shows0Percent()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 0,
            CompletedTasks = 0,
            InProgressTasks = 0,
            CarriedOverTasks = 0,
            EstimatedBurndownRate = 0
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("0%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysProgressBar()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 7,
            InProgressTasks = 2,
            CarriedOverTasks = 1,
            EstimatedBurndownRate = 1.8
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("progress-bar", component.Markup);
        Assert.Contains("70%", component.Markup);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ProgressMetrics_WithPastEndDate_ShowsZeroDaysRemaining()
    {
        var pastDate = DateTime.Now.AddDays(-10);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, DateTime.Now.AddDays(-30))
                .Add(p => p.ProjectEndDate, pastDate)
        );

        Assert.Contains("0 days", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithNullMetrics_HandlesGracefully()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, (ProjectMetrics?)null)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("0%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTimeRemainingSection()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            InProgressTasks = 3,
            CarriedOverTasks = 2,
            EstimatedBurndownRate = 1.5
        };

        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.Metrics, metrics)
                .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
        );

        Assert.Contains("Time Remaining", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_WithVariousCompletionLevels_CalculatesCorrectly()
    {
        var testCases = new[]
        {
            (total: 4, completed: 1, expected: "25%"),
            (total: 4, completed: 2, expected: "50%"),
            (total: 4, completed: 3, expected: "75%"),
            (total: 100, completed: 33, expected: "33%"),
            (total: 7, completed: 3, expected: "42%"),
        };

        foreach (var (total, completed, expected) in testCases)
        {
            var metrics = new ProjectMetrics
            {
                TotalTasks = total,
                CompletedTasks = completed,
                InProgressTasks = 0,
                CarriedOverTasks = 0,
                EstimatedBurndownRate = 1.0
            };

            var component = RenderComponent<ProgressMetrics>(
                parameters => parameters
                    .Add(p => p.Metrics, metrics)
                    .Add(p => p.ProjectStartDate, new DateTime(2024, 1, 1))
                    .Add(p => p.ProjectEndDate, new DateTime(2024, 6, 30))
            );

            Assert.Contains(expected, component.Markup);
        }
    }

    #endregion
}