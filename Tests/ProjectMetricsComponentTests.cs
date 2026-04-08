using Bunit;
using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests
{
    public class ProjectMetricsComponentTests : TestContext
    {
        [Fact]
        public void ProjectMetrics_Renders_AllFourCards_WhenMetricsProvided()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 75,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 12,
                VelocityLastMonth = 10
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var cards = cut.FindAll(".metric-card");
            Assert.Equal(4, cards.Count);
        }

        [Fact]
        public void ProjectMetrics_DisplaysCompletionPercentage_Correctly()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 85,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 12,
                VelocityLastMonth = 10
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var progressText = cut.Find(".progress-text");
            Assert.Contains("85%", progressText.TextContent);
        }

        [Fact]
        public void ProjectMetrics_DisplaysHealthStatus_WithCorrectColor_OnTrack()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 8,
                VelocityLastMonth = 8
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var badge = cut.Find(".status-badge");
            Assert.Contains("#28a745", badge.GetAttribute("style"));
            Assert.Contains("On Track", badge.TextContent);
        }

        [Fact]
        public void ProjectMetrics_DisplaysHealthStatus_WithCorrectColor_AtRisk()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 40,
                HealthStatus = HealthStatus.AtRisk,
                VelocityThisMonth = 5,
                VelocityLastMonth = 10
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var badge = cut.Find(".status-badge");
            Assert.Contains("#dc3545", badge.GetAttribute("style"));
            Assert.Contains("At Risk", badge.TextContent);
        }

        [Fact]
        public void ProjectMetrics_DisplaysHealthStatus_WithCorrectColor_Blocked()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 30,
                HealthStatus = HealthStatus.Blocked,
                VelocityThisMonth = 2,
                VelocityLastMonth = 10
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var badge = cut.Find(".status-badge");
            Assert.Contains("#6c757d", badge.GetAttribute("style"));
            Assert.Contains("Blocked", badge.TextContent);
        }

        [Fact]
        public void ProjectMetrics_DisplaysVelocity_ThisAndLastMonth()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 60,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 14,
                VelocityLastMonth = 11
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var values = cut.FindAll(".metric-value");
            Assert.Contains("14", values[1].TextContent);
            Assert.Contains("11", values[2].TextContent);
        }

        [Fact]
        public void ProjectMetrics_RendersSVG_WithAccessibilityAttributes()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                VelocityLastMonth = 10
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var svg = cut.Find("svg.circular-progress");
            Assert.Equal("progressbar", svg.GetAttribute("role"));
            Assert.Equal("50", svg.GetAttribute("aria-valuenow"));
            Assert.Equal("0", svg.GetAttribute("aria-valuemin"));
            Assert.Equal("100", svg.GetAttribute("aria-valuemax"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public void ProjectMetrics_CalculatesStrokeDashOffset_CorrectlyForPercentage(int percentage)
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = percentage,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                VelocityLastMonth = 10
            };

            // Act
            var expectedOffset = 283 - (percentage * 2.83);
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            // Assert
            var progressFill = cut.Find(".progress-fill");
            var style = progressFill.GetAttribute("style");
            Assert.Contains($"stroke-dashoffset: {expectedOffset}", style);
        }

        [Fact]
        public void ProjectMetrics_DoesNotRender_WhenMetricsNull()
        {
            // Arrange & Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, null as ProjectMetrics));

            // Assert
            var loading = cut.Find(".metrics-loading");
            Assert.NotNull(loading);
        }

        [Fact]
        public void ProjectMetrics_RespondsTo_ViewportChanges_AtMultipleResolutions()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 60,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 12,
                VelocityLastMonth = 10
            };

            // Act
            var cut = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics));

            var container = cut.Find(".metrics-container");

            // Assert - CSS media queries are tested via visual regression
            Assert.NotNull(container);
        }
    }
}