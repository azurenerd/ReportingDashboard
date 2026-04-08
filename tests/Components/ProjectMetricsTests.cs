using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProjectMetricsTests : TestContext
    {
        [Fact]
        public void Renders_EmptyState_WhenMetricsNull()
        {
            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, (ProjectMetrics)null)
            );

            Assert.Contains("No metrics available", component.Markup);
            Assert.Contains("metrics-empty", component.Markup);
        }

        [Fact]
        public void Renders_AllMetricCards()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 75,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 15,
                MilestoneCount = 3,
                TargetMilestoneCount = 5
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("Completion", component.Markup);
            Assert.Contains("Health Status", component.Markup);
            Assert.Contains("Velocity This Month", component.Markup);
            Assert.Contains("Milestones", component.Markup);
        }

        [Fact]
        public void Displays_CompletionPercentage()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 82,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                MilestoneCount = 2,
                TargetMilestoneCount = 4
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("82", component.Markup);
            Assert.Contains("progress-circle", component.Markup);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void Displays_CompletionPercentage_ForVariousValues(int percentage)
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = percentage,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                MilestoneCount = 1,
                TargetMilestoneCount = 2
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains(percentage.ToString(), component.Markup);
        }

        [Fact]
        public void Displays_HealthStatus_OnTrack()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                MilestoneCount = 1,
                TargetMilestoneCount = 2
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("On Track", component.Markup);
            Assert.Contains("data-status=\"ontrack\"", component.Markup);
        }

        [Fact]
        public void Displays_HealthStatus_AtRisk()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 30,
                HealthStatus = HealthStatus.AtRisk,
                VelocityThisMonth = 3,
                MilestoneCount = 1,
                TargetMilestoneCount = 3
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("At Risk", component.Markup);
            Assert.Contains("data-status=\"atrisk\"", component.Markup);
        }

        [Fact]
        public void Displays_HealthStatus_Blocked()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 20,
                HealthStatus = HealthStatus.Blocked,
                VelocityThisMonth = 0,
                MilestoneCount = 0,
                TargetMilestoneCount = 3
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("Blocked", component.Markup);
            Assert.Contains("data-status=\"blocked\"", component.Markup);
        }

        [Fact]
        public void Displays_VelocityThisMonth()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 12,
                MilestoneCount = 2,
                TargetMilestoneCount = 4
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("12", component.Markup);
            Assert.Contains("items", component.Markup);
        }

        [Fact]
        public void Displays_MilestoneProgress()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                MilestoneCount = 2,
                TargetMilestoneCount = 5
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("2", component.Markup);
            Assert.Contains("of 5", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_DisplaysCompletionPercentage()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 65,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 8,
                MilestoneCount = 2,
                TargetMilestoneCount = 3
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("65", component.Markup);
            Assert.Contains("progress-circle", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_ShowsOnTimeStatus()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                MilestoneCount = 1,
                TargetMilestoneCount = 2
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("On Track", component.Markup);
            Assert.Contains("health-badge", component.Markup);
        }

        [Fact]
        public void AcceptanceCriteria_DisplaysVelocityIndicator()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                MilestoneCount = 1,
                TargetMilestoneCount = 2
            };

            var component = RenderComponent<ProjectMetrics>(parameters => 
                parameters.Add(p => p.Metrics, metrics)
            );

            Assert.Contains("10", component.Markup);
            Assert.Contains("metric-number", component.Markup);
        }
    }
}