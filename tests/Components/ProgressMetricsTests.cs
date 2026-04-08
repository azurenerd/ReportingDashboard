using Xunit;
using FluentAssertions;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Components
{
    public class ProgressMetricsTests : TestContext
    {
        [Fact]
        public void ProgressMetrics_DisplaysCompletionPercentage()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 75 };
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics));
            
            component.Markup.Should().Contain("75");
        }

        [Fact]
        public void ProgressMetrics_HandlesNullMetrics_RendersSafely()
        {
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, (ProjectMetrics)null));
            
            component.Markup.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ProgressMetrics_DisplaysProgressBar()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 50 };
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics));
            
            component.Find(".progress-bar").Should().NotBeNull();
        }

        [Fact]
        public void ProgressMetrics_ProgressBarWidth_MatchesPercentage()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 60 };
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics));
            
            var progressBar = component.Find(".progress-bar");
            var style = progressBar.GetAttribute("style");
            
            style.Should().Contain("60%");
        }

        [Fact]
        public void ProgressMetrics_FontSize_MeetsMinimumRequirement()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 50 };
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics));
            
            var percentageElement = component.Find(".percentage-text");
            var computedStyle = percentageElement.GetAttribute("style");
            
            computedStyle.Should().NotBeNullOrEmpty();
            computedStyle.Should().NotContain("font-size: 8pt");
        }

        [Fact]
        public void ProgressMetrics_AppliesResponsiveClass()
        {
            var metrics = new ProjectMetrics { CompletionPercentage = 50 };
            var component = RenderComponent<ProgressMetrics>(parameters =>
                parameters.Add(p => p.Metrics, metrics));
            
            component.Markup.Should().Contain("col-md-4");
        }
    }
}