using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests;

public class ProjectMetricsComponentTests : TestContext
{
    [Fact]
    public void ProjectMetrics_Renders_WithNullMetrics()
    {
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, (ProjectMetrics?)null));

        Assert.NotNull(component);
        var loadingText = component.Markup;
        Assert.Contains("Loading metrics", loadingText);
    }

    [Fact]
    public void ProjectMetrics_Renders_WithValidMetrics()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 12,
            TotalMilestones = 8,
            CompletedMilestones = 3
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        Assert.NotNull(component);
        var markup = component.Markup;
        Assert.Contains("45", markup);
        Assert.Contains("12", markup);
        Assert.Contains("3", markup);
        Assert.Contains("8", markup);
    }

    [Fact]
    public void ProjectMetrics_Renders_AllFourCards()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.AtRisk,
            VelocityThisMonth = 15,
            TotalMilestones = 10,
            CompletedMilestones = 5
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("metric-completion", markup);
        Assert.Contains("metric-status", markup);
        Assert.Contains("metric-velocity", markup);
        Assert.Contains("metric-milestones", markup);
    }

    [Fact]
    public void ProjectMetrics_Renders_MetricLabels()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("Completion", markup);
        Assert.Contains("Project Status", markup);
        Assert.Contains("Velocity", markup);
        Assert.Contains("Milestones", markup);
    }

    [Fact]
    public void ProjectMetrics_HealthBadge_OnTrack_HasCorrectStatus()
    {
        var metrics = new ProjectMetrics
        {
            HealthStatus = HealthStatus.OnTrack
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("data-status=\"ontrack\"", markup);
        Assert.Contains("On Track", markup);
    }

    [Fact]
    public void ProjectMetrics_HealthBadge_AtRisk_HasCorrectStatus()
    {
        var metrics = new ProjectMetrics
        {
            HealthStatus = HealthStatus.AtRisk
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("data-status=\"atrisk\"", markup);
        Assert.Contains("At Risk", markup);
    }

    [Fact]
    public void ProjectMetrics_HealthBadge_Blocked_HasCorrectStatus()
    {
        var metrics = new ProjectMetrics
        {
            HealthStatus = HealthStatus.Blocked
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("data-status=\"blocked\"", markup);
        Assert.Contains("Blocked", markup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void ProjectMetrics_CompletionPercentage_DisplaysCorrectValue(int percentage)
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = percentage,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 0,
            TotalMilestones = 0,
            CompletedMilestones = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains(percentage.ToString(), markup);
    }

    [Fact]
    public void ProjectMetrics_VelocityCard_DisplaysCorrectValue()
    {
        var metrics = new ProjectMetrics
        {
            VelocityThisMonth = 42,
            HealthStatus = HealthStatus.OnTrack,
            CompletionPercentage = 0,
            TotalMilestones = 0,
            CompletedMilestones = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("42", markup);
        Assert.Contains("items this month", markup);
    }

    [Fact]
    public void ProjectMetrics_MilestoneCard_DisplaysProgressRatio()
    {
        var metrics = new ProjectMetrics
        {
            TotalMilestones = 10,
            CompletedMilestones = 7,
            HealthStatus = HealthStatus.OnTrack,
            CompletionPercentage = 0,
            VelocityThisMonth = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("7", markup);
        Assert.Contains("10", markup);
        Assert.Contains("completed", markup);
    }

    [Fact]
    public void ProjectMetrics_ContainerHasCorrectCssClass()
    {
        var metrics = new ProjectMetrics
        {
            HealthStatus = HealthStatus.OnTrack,
            CompletionPercentage = 0,
            VelocityThisMonth = 0,
            TotalMilestones = 0,
            CompletedMilestones = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("metrics-container", markup);
    }

    [Fact]
    public void ProjectMetrics_ContainerHasHealthStatusAttribute()
    {
        var metrics = new ProjectMetrics
        {
            HealthStatus = HealthStatus.AtRisk,
            CompletionPercentage = 0,
            VelocityThisMonth = 0,
            TotalMilestones = 0,
            CompletedMilestones = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("data-health-status=\"atrisk\"", markup);
    }

    [Fact]
    public void ProjectMetrics_ParameterBinding_UpdatesContent()
    {
        var initialMetrics = new ProjectMetrics
        {
            CompletionPercentage = 25,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 5,
            TotalMilestones = 10,
            CompletedMilestones = 1
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, initialMetrics));

        var initialMarkup = component.Markup;
        Assert.Contains("25", initialMarkup);
        Assert.Contains("5", initialMarkup);

        var updatedMetrics = new ProjectMetrics
        {
            CompletionPercentage = 80,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 20,
            TotalMilestones = 10,
            CompletedMilestones = 8
        };

        component.SetParametersAsync(parameters => parameters
            .Add(p => p.Metrics, updatedMetrics));

        var updatedMarkup = component.Markup;
        Assert.Contains("80", updatedMarkup);
        Assert.Contains("20", updatedMarkup);
    }

    [Fact]
    public void ProjectMetrics_RendersWith_ZeroCompletion()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 0,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 0,
            TotalMilestones = 5,
            CompletedMilestones = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("0", markup);
        Assert.DoesNotContain("Loading metrics", markup);
    }

    [Fact]
    public void ProjectMetrics_RendersWith_FullCompletion()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 100,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 50,
            TotalMilestones = 5,
            CompletedMilestones = 5
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("100", markup);
        Assert.Contains("On Track", markup);
    }

    [Fact]
    public void ProjectMetrics_ContainsSemanticHtml()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 15,
            TotalMilestones = 8,
            CompletedMilestones = 4
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("<section", markup);
        Assert.Contains("<article", markup);
        Assert.Contains("<h3", markup);
    }

    [Fact]
    public void ProjectMetrics_DescriptionTextPresent()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 15,
            TotalMilestones = 8,
            CompletedMilestones = 4
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("of project scope", markup);
        Assert.Contains("current health", markup);
        Assert.Contains("items this month", markup);
    }
}