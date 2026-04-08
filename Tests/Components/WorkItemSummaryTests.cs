using System.Collections.Generic;
using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    /// <summary>
    /// Unit tests for WorkItemSummary component behavior.
    /// Validates grouping, truncation, empty state, and rendering logic.
    /// </summary>
    public class WorkItemSummaryTests : TestContext
    {
        [Fact]
        public void RenderThreeColumnsWithCorrectHeaders()
        {
            // Arrange
            var workItems = new List<WorkItem>
            {
                new() { Title = "Item 1", Description = "Desc 1", Status = WorkItemStatus.Shipped },
                new() { Title = "Item 2", Description = "Desc 2", Status = WorkItemStatus.InProgress },
                new() { Title = "Item 3", Description = "Desc 3", Status = WorkItemStatus.CarriedOver }
            };

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, workItems));

            // Assert
            cut.FindAll(".column-header h3").Should().HaveCount(3);
            cut.FindAll(".column-header h3")[0].TextContent.Should().Contain("Shipped This Month");
            cut.FindAll(".column-header h3")[1].TextContent.Should().Contain("In Progress");
            cut.FindAll(".column-header h3")[2].TextContent.Should().Contain("Carried Over");
        }

        [Fact]
        public void DisplayCorrectItemCountsPerColumn()
        {
            // Arrange
            var workItems = new List<WorkItem>
            {
                new() { Title = "S1", Status = WorkItemStatus.Shipped },
                new() { Title = "S2", Status = WorkItemStatus.Shipped },
                new() { Title = "I1", Status = WorkItemStatus.InProgress },
                new() { Title = "C1", Status = WorkItemStatus.CarriedOver }
            };

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, workItems));

            // Assert
            var counts = cut.FindAll(".item-count");
            counts.Should().HaveCount(3);
            counts[0].TextContent.Trim().Should().Be("2");
            counts[1].TextContent.Trim().Should().Be("1");
            counts[2].TextContent.Trim().Should().Be("1");
        }

        [Fact]
        public void TruncateDescriptionsOver100Characters()
        {
            // Arrange
            var longDescription = new string('x', 150);
            var workItems = new List<WorkItem>
            {
                new() { Title = "Item", Description = longDescription, Status = WorkItemStatus.Shipped }
            };

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, workItems));

            // Assert
            var description = cut.Find(".item-description").TextContent;
            description.Length.Should().Be(103); // 100 chars + "..."
            description.Should().EndWith("...");
        }

        [Fact]
        public void HandleNullDescription()
        {
            // Arrange
            var workItems = new List<WorkItem>
            {
                new() { Title = "Item", Description = null, Status = WorkItemStatus.Shipped }
            };

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, workItems));

            // Assert
            var description = cut.Find(".item-description").TextContent;
            description.Should().Contain("No description provided");
        }

        [Fact]
        public void DisplayNoItemsMessageForEmptyColumns()
        {
            // Arrange
            var workItems = new List<WorkItem>
            {
                new() { Title = "Item", Status = WorkItemStatus.Shipped }
            };

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, workItems));

            // Assert
            cut.FindAll(".no-items").Should().HaveCount(2);
            cut.FindAll(".no-items")[0].TextContent.Should().Contain("No items in this category");
        }

        [Fact]
        public void RenderEmptyListGracefully()
        {
            // Arrange
            var workItems = new List<WorkItem>();

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, workItems));

            // Assert
            cut.FindAll(".no-items").Should().HaveCount(3);
            cut.FindAll(".work-item").Should().HaveCount(0);
        }

        [Fact]
        public void HandleNullWorkItemsList()
        {
            // Arrange - no WorkItems parameter passed

            // Act
            var cut = RenderComponent<WorkItemSummary>(parameters =>
                parameters.Add(p => p.WorkItems, (List<WorkItem>)null));

            // Assert
            cut.FindAll(".no-items").Should().HaveCount(3);
        }
    }
}