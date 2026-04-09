using Xunit;
using AgentSquad.Dashboard.Components;

namespace AgentSquad.Dashboard.Tests.Unit.Components
{
    [Trait("Category", "Unit")]
    public class ProgressMetricsTests
    {
        [Fact]
        public void CompletionPercentage_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletionPercentage = 50 };

            // Act
            var result = metrics.CompletionPercentage;

            // Assert
            Assert.Equal(50, result);
        }

        [Fact]
        public void CompletionPercentage_WithZero_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletionPercentage = 0 };

            // Act
            var result = metrics.CompletionPercentage;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CompletionPercentage_WithHundred_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletionPercentage = 100 };

            // Act
            var result = metrics.CompletionPercentage;

            // Assert
            Assert.Equal(100, result);
        }

        [Fact]
        public void CompletedTasks_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletedTasks = 5 };

            // Act
            var result = metrics.CompletedTasks;

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void CompletedTasks_WithZero_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletedTasks = 0 };

            // Act
            var result = metrics.CompletedTasks;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void TotalTasks_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var metrics = new ProgressMetrics { TotalTasks = 10 };

            // Act
            var result = metrics.TotalTasks;

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void TotalTasks_WithZero_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { TotalTasks = 0 };

            // Act
            var result = metrics.TotalTasks;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void BurndownRate_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var metrics = new ProgressMetrics { BurndownRate = 2.5 };

            // Act
            var result = metrics.BurndownRate;

            // Assert
            Assert.Equal(2.5, result);
        }

        [Fact]
        public void BurndownRate_WithZero_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { BurndownRate = 0.0 };

            // Act
            var result = metrics.BurndownRate;

            // Assert
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void BurndownRate_WithFractionalValue_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { BurndownRate = 1.25 };

            // Act
            var result = metrics.BurndownRate;

            // Assert
            Assert.Equal(1.25, result);
        }

        [Fact]
        public void CompletedTasks_GreaterThanZero_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletedTasks = 999 };

            // Act
            var result = metrics.CompletedTasks;

            // Assert
            Assert.Equal(999, result);
        }

        [Fact]
        public void TotalTasks_GreaterThanZero_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { TotalTasks = 999 };

            // Act
            var result = metrics.TotalTasks;

            // Assert
            Assert.Equal(999, result);
        }

        [Fact]
        public void CompletionPercentage_NegativeValue_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { CompletionPercentage = -10 };

            // Act
            var result = metrics.CompletionPercentage;

            // Assert
            Assert.Equal(-10, result);
        }

        [Fact]
        public void BurndownRate_NegativeValue_CanBeSet()
        {
            // Arrange
            var metrics = new ProgressMetrics { BurndownRate = -1.5 };

            // Act
            var result = metrics.BurndownRate;

            // Assert
            Assert.Equal(-1.5, result);
        }
    }
}