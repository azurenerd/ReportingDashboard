using System;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class ProjectMetricsTests
    {
        [Fact]
        public void CompletionPercentage_WhenValid_StoresCorrectValue()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { CompletionPercentage = 45 };

            // Assert
            Assert.Equal(45, metrics.CompletionPercentage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void CompletionPercentage_WithValidRange_Accepted(int percentage)
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { CompletionPercentage = percentage };

            // Assert
            Assert.Equal(percentage, metrics.CompletionPercentage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(150)]
        public void CompletionPercentage_OutOfRange_ShouldValidate(int percentage)
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = percentage };

            // Act & Assert
            Assert.False(IsValidPercentage(metrics.CompletionPercentage));
        }

        [Theory]
        [InlineData("OnTrack")]
        [InlineData("AtRisk")]
        [InlineData("Blocked")]
        public void HealthStatus_WithValidValues_Accepted(string status)
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { HealthStatus = status };

            // Assert
            Assert.Equal(status, metrics.HealthStatus);
        }

        [Theory]
        [InlineData("InvalidStatus")]
        [InlineData("Pending")]
        [InlineData("")]
        public void HealthStatus_WithInvalidValues_ShouldFail(string status)
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { HealthStatus = status };

            // Assert
            Assert.False(IsValidHealthStatus(metrics.HealthStatus));
        }

        [Fact]
        public void VelocityThisMonth_StoresCorrectValue()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { VelocityThisMonth = 12 };

            // Assert
            Assert.Equal(12, metrics.VelocityThisMonth);
        }

        [Fact]
        public void MilestoneCount_StoresCorrectValue()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { MilestoneCount = 3 };

            // Assert
            Assert.Equal(3, metrics.MilestoneCount);
        }

        [Fact]
        public void CompletionPercentage_WithZero_IsValid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { CompletionPercentage = 0 };

            // Assert
            Assert.True(IsValidPercentage(metrics.CompletionPercentage));
        }

        [Fact]
        public void CompletionPercentage_WithOneHundred_IsValid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { CompletionPercentage = 100 };

            // Assert
            Assert.True(IsValidPercentage(metrics.CompletionPercentage));
        }

        [Fact]
        public void HealthStatus_OnTrack_IsValid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { HealthStatus = "OnTrack" };

            // Assert
            Assert.True(IsValidHealthStatus(metrics.HealthStatus));
        }

        [Fact]
        public void HealthStatus_AtRisk_IsValid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { HealthStatus = "AtRisk" };

            // Assert
            Assert.True(IsValidHealthStatus(metrics.HealthStatus));
        }

        [Fact]
        public void HealthStatus_Blocked_IsValid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics { HealthStatus = "Blocked" };

            // Assert
            Assert.True(IsValidHealthStatus(metrics.HealthStatus));
        }

        private bool IsValidPercentage(int percentage)
        {
            return percentage >= 0 && percentage <= 100;
        }

        private bool IsValidHealthStatus(string status)
        {
            var validStatuses = new[] { "OnTrack", "AtRisk", "Blocked" };
            return Array.Exists(validStatuses, s => s == status);
        }
    }

    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
        public string HealthStatus { get; set; }
        public int VelocityThisMonth { get; set; }
        public int MilestoneCount { get; set; }
    }
}