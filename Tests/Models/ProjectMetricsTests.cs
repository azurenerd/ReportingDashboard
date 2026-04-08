using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models
{
    public class ProjectMetricsTests
    {
        [Fact]
        public void ProjectMetrics_WithValidData_InitializesSuccessfully()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 75,
                HealthStatus = HealthStatus.OnTrack,
                VelocityCount = 12
            };

            Assert.Equal(75, metrics.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
            Assert.Equal(12, metrics.VelocityCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void ProjectMetrics_AcceptsValidCompletionPercentages(int percentage)
        {
            var metrics = new ProjectMetrics { CompletionPercentage = percentage };
            Assert.Equal(percentage, metrics.CompletionPercentage);
        }

        [Theory]
        [InlineData(HealthStatus.OnTrack)]
        [InlineData(HealthStatus.AtRisk)]
        [InlineData(HealthStatus.Blocked)]
        public void ProjectMetrics_AcceptsAllHealthStatuses(HealthStatus status)
        {
            var metrics = new ProjectMetrics { HealthStatus = status };
            Assert.Equal(status, metrics.HealthStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public void ProjectMetrics_AcceptsAnyNonNegativeVelocity(int velocity)
        {
            var metrics = new ProjectMetrics { VelocityCount = velocity };
            Assert.Equal(velocity, metrics.VelocityCount);
        }

        [Fact]
        public void ProjectMetrics_CanRepresentZeroCompletion()
        {
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = 0,
                HealthStatus = HealthStatus.AtRisk,
                VelocityCount = 0
            };

            Assert.Equal(0, metrics.CompletionPercentage);
            Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
            Assert.Equal(0, metrics.VelocityCount);
        }
    }
}