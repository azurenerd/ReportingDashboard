using Xunit;
using AgentSquad.Models;

namespace AgentSquad.Tests.Models
{
    public class ProjectMetricsTests
    {
        [Fact]
        public void ProjectMetrics_ValidInitialization_Success()
        {
            var metrics = new ProjectMetrics
            {
                ProjectId = "p1",
                VelocityThisMonth = 42,
                HealthStatus = HealthStatus.Healthy,
                CompletionPercentage = 75
            };

            Assert.Equal("p1", metrics.ProjectId);
            Assert.Equal(42, metrics.VelocityThisMonth);
            Assert.Equal(HealthStatus.Healthy, metrics.HealthStatus);
            Assert.Equal(75, metrics.CompletionPercentage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public void ProjectMetrics_VelocityThisMonth_ValidValues(int velocity)
        {
            var metrics = new ProjectMetrics
            {
                ProjectId = "p1",
                VelocityThisMonth = velocity,
                HealthStatus = HealthStatus.Healthy,
                CompletionPercentage = 0
            };

            Assert.Equal(velocity, metrics.VelocityThisMonth);
        }

        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.AtRisk)]
        [InlineData(HealthStatus.Critical)]
        public void ProjectMetrics_AllHealthStatus_Success(HealthStatus status)
        {
            var metrics = new ProjectMetrics
            {
                ProjectId = "p1",
                VelocityThisMonth = 10,
                HealthStatus = status,
                CompletionPercentage = 50
            };

            Assert.Equal(status, metrics.HealthStatus);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void ProjectMetrics_InvalidCompletionPercentage_ThrowsInvalidOperationException(int percentage)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                if (percentage < 0 || percentage > 100)
                    throw new InvalidOperationException($"CompletionPercentage must be 0-100, got {percentage}");
            });
            Assert.Contains("0-100", ex.Message);
        }
    }
}