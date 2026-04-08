using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProjectMetricsTests : TestContext
    {
        private ProjectMetrics CreateTestMetrics()
        {
            return new ProjectMetrics
            {
                CompletionPercentage = 65,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 15,
                TotalMilestones = 4,
                CompletedMilestones = 2
            };
        }

        [Fact]
        public void RenderProjectMetrics_WithValidMetrics()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var metricsGrid = cut.Find(".metrics-grid");
            Assert.NotNull(metricsGrid);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysCompletionPercentage()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("65%", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysHealthStatus()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("On Track", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysVelocity()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("15", cut.Markup);
            Assert.Contains("items", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysMilestoneCount()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("2", cut.Markup);
            Assert.Contains("4", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysProgressBar()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var progressFill = cut.Find(".progress-fill");
            Assert.NotNull(progressFill);
        }

        [Fact]
        public void RenderProjectMetrics_ProgressBarWidthMatchesPercentage()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var progressFill = cut.Find(".progress-fill");
            var style = progressFill.GetAttribute("style");
            Assert.Contains("65%", style);
        }

        [Fact]
        public void RenderProjectMetrics_HealthBadgeOnTrack()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 10,
                TotalMilestones = 2,
                CompletedMilestones = 1
            };
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var badge = cut.Find(".health-badge[data-health='ontrack']");
            Assert.NotNull(badge);
            Assert.Contains("On Track", badge.TextContent);
        }

        [Fact]
        public void RenderProjectMetrics_HealthBadgeAtRisk()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.AtRisk,
                VelocityThisMonth = 5,
                TotalMilestones = 2,
                CompletedMilestones = 1
            };
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var badge = cut.Find(".health-badge[data-health='atrisk']");
            Assert.NotNull(badge);
            Assert.Contains("At Risk", badge.TextContent);
        }

        [Fact]
        public void RenderProjectMetrics_HealthBadgeBlocked()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 30,
                HealthStatus = HealthStatus.Blocked,
                VelocityThisMonth = 0,
                TotalMilestones = 2,
                CompletedMilestones = 0
            };
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var badge = cut.Find(".health-badge[data-health='blocked']");
            Assert.NotNull(badge);
            Assert.Contains("Blocked", badge.TextContent);
        }

        [Fact]
        public void RenderProjectMetrics_DoesNotRender_WhenMetricsNull()
        {
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, (ProjectMetrics)null));

            cut.Render();
            var metricsGrid = cut.QuerySelector(".metrics-grid");
            Assert.Null(metricsGrid);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysMetricCards()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            var cards = cut.FindAll(".metric-card");
            Assert.Equal(4, cards.Count);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysCompletionLabel()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("Completion", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysHealthLabel()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("Health Status", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysVelocityLabel()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("This Month Velocity", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_DisplaysMilestonesLabel()
        {
            var metrics = CreateTestMetrics();
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("Milestones", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_ZeroCompletion()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 0,
                HealthStatus = HealthStatus.AtRisk,
                VelocityThisMonth = 0,
                TotalMilestones = 5,
                CompletedMilestones = 0
            };
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("0%", cut.Markup);
        }

        [Fact]
        public void RenderProjectMetrics_MaxCompletion()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 100,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 30,
                TotalMilestones = 4,
                CompletedMilestones = 4
            };
            var cut = RenderComponent<ProjectMetrics>(parameters => parameters
                .Add(p => p.Metrics, metrics));

            cut.Render();
            Assert.Contains("100%", cut.Markup);
        }
    }
}