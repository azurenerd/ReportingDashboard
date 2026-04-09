using System;
using System.Collections.Generic;
using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Services
{
    public class WorkItemCalculationsTests
    {
        [Fact]
        public void MapWorkItemViewModels_WithNullItems_ThrowsArgumentNullException()
        {
            // Arrange
            var milestones = new List<Milestone>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                WorkItemCalculations.MapWorkItemViewModels(null, milestones));
        }

        [Fact]
        public void MapWorkItemViewModels_WithNullMilestones_ThrowsArgumentNullException()
        {
            // Arrange
            var items = new List<WorkItem>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                WorkItemCalculations.MapWorkItemViewModels(items, null));
        }

        [Fact]
        public void MapWorkItemViewModels_WithEmptyItems_ReturnsEmptyList()
        {
            // Arrange
            var items = new List<WorkItem>();
            var milestones = new List<Milestone>();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Empty(result);
        }

        // Additional test methods will be added in Step 2
    }
}