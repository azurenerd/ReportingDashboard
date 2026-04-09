using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Services
{
    public class WorkItemCalculationsTests
    {
        private static List<Milestone> CreateSampleMilestones()
        {
            return new List<Milestone>
            {
                new Milestone { Id = "m1", Name = "Phase 1", DueDate = new DateTime(2026, 04, 15), Status = "on-track" },
                new Milestone { Id = "m2", Name = "Phase 2", DueDate = new DateTime(2026, 05, 15), Status = "at-risk" },
                new Milestone { Id = "m3", Name = "Phase 3", DueDate = new DateTime(2026, 06, 15), Status = "completed" }
            };
        }

        private static List<WorkItem> CreateSampleWorkItems()
        {
            return new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", Description = "Description 1", MilestoneId = "m1", Owner = "Alice", CompletionDate = new DateTime(2026, 04, 10), Status = "shipped" },
                new WorkItem { Id = "wi2", Title = "Item 2", Description = "Description 2", MilestoneId = "m2", Owner = "Bob", CompletionDate = null, Status = "in-progress" },
                new WorkItem { Id = "wi3", Title = "Item 3", Description = "Description 3", MilestoneId = "m3", Owner = "Charlie", CompletionDate = null, Status = "carryover" }
            };
        }

        [Fact]
        public void MapWorkItemViewModels_WithNullItems_ThrowsArgumentNullException()
        {
            // Arrange
            var milestones = CreateSampleMilestones();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                WorkItemCalculations.MapWorkItemViewModels(null, milestones));
            Assert.Equal("items", ex.ParamName);
        }

        [Fact]
        public void MapWorkItemViewModels_WithNullMilestones_ThrowsArgumentNullException()
        {
            // Arrange
            var items = CreateSampleWorkItems();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                WorkItemCalculations.MapWorkItemViewModels(items, null));
            Assert.Equal("milestones", ex.ParamName);
        }

        [Fact]
        public void MapWorkItemViewModels_WithEmptyItems_ReturnsEmptyList()
        {
            // Arrange
            var items = new List<WorkItem>();
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void MapWorkItemViewModels_WithValidMilestoneId_ResolvesCorrectMilestoneName()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "m1", Owner = "Alice", Status = "shipped" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            Assert.Equal("Phase 1", result[0].MilestoneName);
        }

        [Fact]
        public void MapWorkItemViewModels_WithMissingMilestoneId_DisplaysUnknownMilestone()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "m999", Owner = "Alice", Status = "shipped" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            Assert.Equal("Unknown Milestone", result[0].MilestoneName);
        }

        [Fact]
        public void MapWorkItemViewModels_WithNullMilestoneIdInItem_DisplaysUnknownMilestone()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = null, Owner = "Alice", Status = "shipped" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            Assert.Equal("Unknown Milestone", result[0].MilestoneName);
        }

        [Fact]
        public void MapWorkItemViewModels_WithEmptyMilestonesList_DisplaysUnknownMilestone()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "m1", Owner = "Alice", Status = "shipped" }
            };
            var milestones = new List<Milestone>();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            Assert.Equal("Unknown Milestone", result[0].MilestoneName);
        }

        [Fact]
        public void MapWorkItemViewModels_WithMultipleItems_AppliesAlternatingCssClasses()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "m1", Owner = "Alice", Status = "shipped" },
                new WorkItem { Id = "wi2", Title = "Item 2", MilestoneId = "m2", Owner = "Bob", Status = "in-progress" },
                new WorkItem { Id = "wi3", Title = "Item 3", MilestoneId = "m3", Owner = "Charlie", Status = "carryover" },
                new WorkItem { Id = "wi4", Title = "Item 4", MilestoneId = "m1", Owner = "David", Status = "shipped" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal("row-even", result[0].RowCssClass);
            Assert.Equal("row-odd", result[1].RowCssClass);
            Assert.Equal("row-even", result[2].RowCssClass);
            Assert.Equal("row-odd", result[3].RowCssClass);
        }

        [Fact]
        public void MapWorkItemViewModels_WithMultipleMilestoneIds_ResolvesEachCorrectly()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "m1", Owner = "Alice", Status = "shipped" },
                new WorkItem { Id = "wi2", Title = "Item 2", MilestoneId = "m2", Owner = "Bob", Status = "in-progress" },
                new WorkItem { Id = "wi3", Title = "Item 3", MilestoneId = "m3", Owner = "Charlie", Status = "carryover" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Phase 1", result[0].MilestoneName);
            Assert.Equal("Phase 2", result[1].MilestoneName);
            Assert.Equal("Phase 3", result[2].MilestoneName);
        }

        [Fact]
        public void MapWorkItemViewModels_PreservesOriginalItemProperties()
        {
            // Arrange
            var completionDate = new DateTime(2026, 04, 10);
            var items = new List<WorkItem>
            {
                new WorkItem 
                { 
                    Id = "wi1", 
                    Title = "Test Item", 
                    Description = "Test Description",
                    MilestoneId = "m1", 
                    Owner = "Alice",
                    CompletionDate = completionDate,
                    Status = "shipped"
                }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            var viewModel = result[0];
            Assert.Equal("wi1", viewModel.Id);
            Assert.Equal("Test Item", viewModel.Title);
            Assert.Equal("Test Description", viewModel.Description);
            Assert.Equal("Alice", viewModel.Owner);
            Assert.Equal(completionDate, viewModel.CompletionDate);
            Assert.Equal("shipped", viewModel.Status);
        }

        [Fact]
        public void MapWorkItemViewModels_WithWhitespaceMilestoneId_DisplaysUnknownMilestone()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "   ", Owner = "Alice", Status = "shipped" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            Assert.Equal("Unknown Milestone", result[0].MilestoneName);
        }

        [Fact]
        public void MapWorkItemViewModels_ReturnsCorrectCount()
        {
            // Arrange
            var items = CreateSampleWorkItems();
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Equal(items.Count, result.Count);
        }

        [Fact]
        public void MapWorkItemViewModels_InitializesRiskBadgeAsEmpty()
        {
            // Arrange
            var items = new List<WorkItem>
            {
                new WorkItem { Id = "wi1", Title = "Item 1", MilestoneId = "m1", Owner = "Alice", Status = "shipped" }
            };
            var milestones = CreateSampleMilestones();

            // Act
            var result = WorkItemCalculations.MapWorkItemViewModels(items, milestones);

            // Assert
            Assert.Single(result);
            Assert.Empty(result[0].RiskBadge);
        }
    }
}