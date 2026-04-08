using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components;

public class ProjectMetricsComponentTests : TestContext
{
    [Fact]
    public void Renders_LoadingPlaceholder_WhenMetricsNull()
    {
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, (ProjectMetrics?)null));

        Assert.NotNull(component);
        var markup = component.Markup;
        Assert.Contains("metrics-loading", markup);
        Assert.Contains("Loading metrics", markup);
    }

    [Fact]
    public void Renders_AllFourMetricCards_WhenMetricsProvided()
    {
        var metrics = CreateValidMetrics();
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("metric-completion", markup);
        Assert.Contains("metric-status", markup);
        Assert.Contains("metric-velocity", markup);
        Assert.Contains("metric-milestones", markup);
    }

    [Fact]
    public void Displays_CompletionPercentage_WithProgressCircle()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("45%", markup);
        Assert.Contains("progress-circle", markup);
        Assert.Contains("progress-value", markup);
    }

    [Fact]
    public void Displays_HealthStatusBadge_WithCorrectLabel_OnTrack()
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
        Assert.Contains("health-badge", markup);
        Assert.Contains("On Track", markup);
        Assert.Contains("data-status=\"ontrack\"", markup);
    }

    [Fact]
    public void Displays_HealthStatusBadge_WithCorrectLabel_AtRisk()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.AtRisk,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("At Risk", markup);
        Assert.Contains("data-status=\"atrisk\"", markup);
    }

    [Fact]
    public void Displays_HealthStatusBadge_WithCorrectLabel_Blocked()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("Blocked", markup);
        Assert.Contains("data-status=\"blocked\"", markup);
    }

    [Fact]
    public void Displays_VelocityValue_WithItemsThisMonthLabel()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 12,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("12", markup);
        Assert.Contains("items this month", markup);
    }

    [Fact]
    public void Displays_MilestoneProgress_AsRatio()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            TotalMilestones = 8,
            CompletedMilestones = 3
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("3", markup);
        Assert.Contains("8", markup);
        Assert.Contains("completed", markup);
    }

    [Fact]
    public void Renders_MetricLabels_ForAllCards()
    {
        var metrics = CreateValidMetrics();
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("Completion", markup);
        Assert.Contains("Project Status", markup);
        Assert.Contains("Velocity", markup);
        Assert.Contains("Milestones", markup);
    }

    [Fact]
    public void Container_HasHealthStatusAttribute()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.AtRisk,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("data-health-status=\"atrisk\"", markup);
        Assert.Contains("metrics-container", markup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void Displays_CompletionPercentage_VariousValues(int percentage)
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = percentage,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains($"{percentage}%", markup);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Displays_VelocityValue_VariousValues(int velocity)
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = velocity,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains(velocity.ToString(), markup);
    }

    [Fact]
    public void Displays_MilestoneBoundary_ZeroCompleted()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 0,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 0,
            TotalMilestones = 10,
            CompletedMilestones = 0
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("0", markup);
        Assert.Contains("10", markup);
    }

    [Fact]
    public void Displays_MilestoneBoundary_AllCompleted()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 100,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 25,
            TotalMilestones = 5,
            CompletedMilestones = 5
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("5", markup);
    }

    [Fact]
    public void Parameter_Binding_UpdatesContent_OnChange()
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
        Assert.Contains("25%", initialMarkup);

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
        Assert.Contains("80%", updatedMarkup);
        Assert.DoesNotContain("25%", updatedMarkup);
    }

    [Fact]
    public void Renders_MetricCards_WithSemanticHTML()
    {
        var metrics = CreateValidMetrics();
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("<section", markup);
        Assert.Contains("<article", markup);
        Assert.Contains("class=\"metric-card\"", markup);
    }

    [Fact]
    public void Renders_MetricHeader_WithHeading()
    {
        var metrics = CreateValidMetrics();
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("<h3", markup);
        Assert.Contains("metric-label", markup);
    }

    [Fact]
    public void Renders_MetricFooter_WithDescription()
    {
        var metrics = CreateValidMetrics();
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("metric-footer", markup);
        Assert.Contains("metric-description", markup);
    }

    [Fact]
    public void NullMetrics_DisplaysLoadingState_NotErrorState()
    {
        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, (ProjectMetrics?)null));

        var markup = component.Markup;
        Assert.Contains("metric-loading", markup);
        Assert.DoesNotContain("error", markup.ToLower());
    }

    [Fact]
    public void CompletionPercentage_ProgressIndicator_HasInlineStyle()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 65,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var component = RenderComponent<ProjectMetrics>(parameters => parameters
            .Add(p => p.Metrics, metrics));

        var markup = component.Markup;
        Assert.Contains("--completion-percentage:", markup);
        Assert.Contains("65%", markup);
    }

    private ProjectMetrics CreateValidMetrics() => new()
    {
        CompletionPercentage = 45,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 12,
        TotalMilestones = 8,
        CompletedMilestones = 3
    };
}