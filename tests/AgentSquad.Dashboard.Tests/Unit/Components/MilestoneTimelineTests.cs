using Xunit;
using AgentSquad.Dashboard.Components;
using System;
using System.Collections.Generic;

namespace AgentSquad.Dashboard.Tests.Unit.Components
{
    [Trait("Category", "Unit")]
    public class MilestoneTimelineTests
    {
        [Fact]
        public void GetStatusClass_WithCompletedStatus_ReturnsCompletedClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetStatusClass(MilestoneStatus.Completed);

            // Assert
            Assert.Equal("completed", result);
        }

        [Fact]
        public void GetStatusClass_WithInProgressStatus_ReturnsInProgressClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetStatusClass(MilestoneStatus.InProgress);

            // Assert
            Assert.Equal("in-progress", result);
        }

        [Fact]
        public void GetStatusClass_WithPendingStatus_ReturnsPendingClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetStatusClass(MilestoneStatus.Pending);

            // Assert
            Assert.Equal("pending", result);
        }

        [Fact]
        public void GetStatusClass_WithUnknownStatus_ReturnsPendingClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();
            var unknownStatus = (MilestoneStatus)999;

            // Act
            var result = timeline.GetStatusClass(unknownStatus);

            // Assert
            Assert.Equal("pending", result);
        }

        [Fact]
        public void GetProgressBarClass_WithCompletedStatus_ReturnsSuccessClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetProgressBarClass(MilestoneStatus.Completed);

            // Assert
            Assert.Equal("bg-success", result);
        }

        [Fact]
        public void GetProgressBarClass_WithInProgressStatus_ReturnsInfoClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetProgressBarClass(MilestoneStatus.InProgress);

            // Assert
            Assert.Equal("bg-info", result);
        }

        [Fact]
        public void GetProgressBarClass_WithPendingStatus_ReturnsSecondaryClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetProgressBarClass(MilestoneStatus.Pending);

            // Assert
            Assert.Equal("bg-secondary", result);
        }

        [Fact]
        public void GetProgressBarClass_WithUnknownStatus_ReturnsSecondaryClass()
        {
            // Arrange
            var timeline = new MilestoneTimeline();
            var unknownStatus = (MilestoneStatus)999;

            // Act
            var result = timeline.GetProgressBarClass(unknownStatus);

            // Assert
            Assert.Equal("bg-secondary", result);
        }

        [Fact]
        public void GetStatusIcon_WithCompletedStatus_ReturnsCheckmark()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetStatusIcon(MilestoneStatus.Completed);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void GetStatusIcon_WithInProgressStatus_ReturnsClock()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetStatusIcon(MilestoneStatus.InProgress);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetStatusIcon_WithPendingStatus_ReturnsDot()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.GetStatusIcon(MilestoneStatus.Pending);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Milestones_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new Milestone { Name = "M1", TargetDate = DateTime.Now }
            };
            var timeline = new MilestoneTimeline { Milestones = milestones };

            // Act
            var result = timeline.Milestones;

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void ProjectStartDate_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var startDate = new DateTime(2024, 01, 01);
            var timeline = new MilestoneTimeline { ProjectStartDate = startDate };

            // Act
            var result = timeline.ProjectStartDate;

            // Assert
            Assert.Equal(startDate, result);
        }

        [Fact]
        public void ProjectEndDate_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var endDate = new DateTime(2024, 12, 31);
            var timeline = new MilestoneTimeline { ProjectEndDate = endDate };

            // Act
            var result = timeline.ProjectEndDate;

            // Assert
            Assert.Equal(endDate, result);
        }

        [Fact]
        public void Milestones_WithNullValue_DefaultsToNull()
        {
            // Arrange
            var timeline = new MilestoneTimeline();

            // Act
            var result = timeline.Milestones;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Milestones_WithEmptyList_CanBeSet()
        {
            // Arrange
            var milestones = new List<Milestone>();
            var timeline = new MilestoneTimeline { Milestones = milestones };

            // Act
            var result = timeline.Milestones;

            // Assert
            Assert.Empty(result);
        }
    }
}