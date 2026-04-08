using Xunit;
using Bunit;
using AgentSquad.Dashboard.Components;

namespace AgentSquad.Dashboard.Tests.Components;

public class ProgressMetricsTests : TestContext
{
    [Fact]
    public void ProgressMetrics_DisplaysCompletionPercentage()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 75)
                .Add(p => p.TotalTasks, 10)
                .Add(p => p.CompletedTasks, 7)
        );

        Assert.Contains("75%", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysTaskCounts()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 60)
                .Add(p => p.TotalTasks, 10)
                .Add(p => p.CompletedTasks, 6)
        );

        Assert.Contains("6 of 10", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysProgressBar()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 50)
                .Add(p => p.TotalTasks, 10)
                .Add(p => p.CompletedTasks, 5)
        );

        var progressBar = component.Find(".progress-bar");
        Assert.NotNull(progressBar);
        Assert.Contains("width: 50%", progressBar.OuterHtml);
    }

    [Fact]
    public void ProgressMetrics_HandlesZeroCompletion()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 0)
                .Add(p => p.TotalTasks, 10)
                .Add(p => p.CompletedTasks, 0)
        );

        Assert.Contains("0%", component.Markup);
        Assert.Contains("0 of 10", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_Handles100PercentCompletion()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 100)
                .Add(p => p.TotalTasks, 10)
                .Add(p => p.CompletedTasks, 10)
        );

        Assert.Contains("100%", component.Markup);
        Assert.Contains("10 of 10", component.Markup);
    }

    [Fact]
    public void ProgressMetrics_DisplaysLargePercentageText()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 85)
                .Add(p => p.TotalTasks, 20)
                .Add(p => p.CompletedTasks, 17)
        );

        var largePercentage = component.Find(".large-percentage");
        Assert.NotNull(largePercentage);
        Assert.Contains("85%", largePercentage.InnerHtml);
    }

    [Fact]
    public void ProgressMetrics_DisplaysMetricsContainer()
    {
        var component = RenderComponent<ProgressMetrics>(
            parameters => parameters
                .Add(p => p.CompletionPercentage, 45)
                .Add(p => p.TotalTasks, 20)
                .Add(p => p.CompletedTasks, 9)
        );

        var container = component.Find(".metrics-container");
        Assert.NotNull(container);
    }
}