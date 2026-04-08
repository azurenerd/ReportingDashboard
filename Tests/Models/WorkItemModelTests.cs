using Xunit;
using AgentSquad.Models;

namespace AgentSquad.Tests.Models
{
    /// <summary>
    /// Unit tests for WorkItem model and WorkItemStatus enum.
    /// Validates model structure, enum values, and property initialization.
    /// </summary>
    public class WorkItemModelTests
    {
        [Fact]
        public void WorkItemPropertiesInitializeCorrectly()
        {
            // Arrange & Act
            var workItem = new WorkItem
            {
                Title = "Test Item",
                Description = "Test Description",
                Status = WorkItemStatus.Shipped,
                AssignedTo = "Test User"
            };

            // Assert
            Assert.Equal("Test Item", workItem.Title);
            Assert.Equal("Test Description", workItem.Description);
            Assert.Equal(WorkItemStatus.Shipped, workItem.Status);
            Assert.Equal("Test User", workItem.AssignedTo);
        }

        [Fact]
        public void WorkItemStatusEnumHasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)WorkItemStatus.Shipped);
            Assert.Equal(1, (int)WorkItemStatus.InProgress);
            Assert.Equal(2, (int)WorkItemStatus.CarriedOver);
        }

        [Fact]
        public void WorkItemAllowsNullDescription()
        {
            // Arrange & Act
            var workItem = new WorkItem
            {
                Title = "Test",
                Description = null,
                Status = WorkItemStatus.Shipped
            };

            // Assert
            Assert.Null(workItem.Description);
        }

        [Fact]
        public void WorkItemAllowsNullAssignedTo()
        {
            // Arrange & Act
            var workItem = new WorkItem
            {
                Title = "Test",
                Status = WorkItemStatus.InProgress,
                AssignedTo = null
            };

            // Assert
            Assert.Null(workItem.AssignedTo);
        }
    }
}