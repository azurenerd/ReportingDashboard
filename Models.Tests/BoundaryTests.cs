using System;
using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models.Tests
{
    public class BoundaryTests
    {
        [Fact]
        public void ProjectMetrics_CompletionPercentage_MinimumValue()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = 0 };

            // Act & Assert
            Assert.Equal(0, metrics.CompletionPercentage);
        }

        [Fact]
        public void ProjectMetrics_CompletionPercentage_MaximumValue()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = 100 };

            // Act & Assert
            Assert.Equal(100, metrics.CompletionPercentage);
        }

        [Fact]
        public void ProjectMetrics_CompletionPercentage_MidRangeValue()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = 50 };

            // Act & Assert
            Assert.Equal(50, metrics.CompletionPercentage);
        }

        [Fact]
        public void ProjectMetrics_VelocityThisMonth_ZeroValue()
        {
            // Arrange
            var metrics = new ProjectMetrics { VelocityThisMonth = 0 };

            // Act & Assert
            Assert.Equal(0, metrics.VelocityThisMonth);
        }

        [Fact]
        public void ProjectMetrics_Milestones_CountsValid()
        {
            // Arrange
            var metrics = new ProjectMetrics
            {
                TotalMilestones = 10,
                CompletedMilestones = 5
            };

            // Act & Assert
            Assert.Equal(10, metrics.TotalMilestones);
            Assert.Equal(5, metrics.CompletedMilestones);
            Assert.True(metrics.CompletedMilestones <= metrics.TotalMilestones);
        }

        [Fact]
        public void Milestone_TargetDate_ValidDateTime()
        {
            // Arrange
            var targetDate = new DateTime(2024, 12, 31, 23, 59, 59);
            var milestone = new Milestone { TargetDate = targetDate };

            // Act & Assert
            Assert.Equal(targetDate, milestone.TargetDate);
        }

        [Fact]
        public void Project_StartDate_BeforeTargetEndDate()
        {
            // Arrange
            var project = new Project
            {
                StartDate = new DateTime(2024, 1, 1),
                TargetEndDate = new DateTime(2024, 12, 31)
            };

            // Act & Assert
            Assert.True(project.StartDate < project.TargetEndDate);
        }
    }
}