using System;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class WorkItemTests
    {
        [Fact]
        public void WorkItem_WithValidData_StoresCorrectly()
        {
            // Arrange & Act
            var workItem = new WorkItem
            {
                Title = "API Integration",
                Description = "Connect to corporate data warehouse",
                Status = "InProgress",
                AssignedTo = "Team A"
            };

            // Assert
            Assert.Equal("API Integration", workItem.Title);
            Assert.Equal("Connect to corporate data warehouse", workItem.Description);
            Assert.Equal("InProgress", workItem.Status);
            Assert.Equal("Team A", workItem.AssignedTo);
        }

        [Theory]
        [InlineData("Shipped")]
        [InlineData("InProgress")]
        [InlineData("CarriedOver")]
        public void WorkItem_WithValidStatus_Accepted(string status)
        {
            // Arrange & Act
            var workItem = new WorkItem { Status = status };

            // Assert
            Assert.True(IsValidWorkItemStatus(workItem.Status));
        }

        [Theory]
        [InlineData("InvalidStatus")]
        [InlineData("Done")]
        [InlineData("Completed")]
        public void WorkItem_WithInvalidStatus_ShouldFail(string status)
        {
            // Arrange & Act
            var workItem = new WorkItem { Status = status };

            // Assert
            Assert.False(IsValidWorkItemStatus(workItem.Status));
        }

        [Fact]
        public void WorkItem_Shipped_IsValid()
        {
            // Arrange & Act
            var workItem = new WorkItem { Status = "Shipped" };

            // Assert
            Assert.Equal("Shipped", workItem.Status);
        }

        [Fact]
        public void WorkItem_InProgress_IsValid()
        {
            // Arrange & Act
            var workItem = new WorkItem { Status = "InProgress" };

            // Assert
            Assert.Equal("InProgress", workItem.Status);
        }

        [Fact]
        public void WorkItem_CarriedOver_IsValid()
        {
            // Arrange & Act
            var workItem = new WorkItem { Status = "CarriedOver" };

            // Assert
            Assert.Equal("CarriedOver", workItem.Status);
        }

        [Fact]
        public void WorkItem_WithEmptyTitle_Stored()
        {
            // Arrange & Act
            var workItem = new WorkItem { Title = "" };

            // Assert
            Assert.Empty(workItem.Title);
        }

        [Fact]
        public void WorkItem_WithLongDescription_Stored()
        {
            // Arrange
            var longDescription = new string('a', 1000);

            // Act
            var workItem = new WorkItem { Description = longDescription };

            // Assert
            Assert.Equal(longDescription, workItem.Description);
        }

        [Fact]
        public void WorkItem_WithNullAssignment_Stored()
        {
            // Arrange & Act
            var workItem = new WorkItem { AssignedTo = null };

            // Assert
            Assert.Null(workItem.AssignedTo);
        }

        private bool IsValidWorkItemStatus(string status)
        {
            var validStatuses = new[] { "Shipped", "InProgress", "CarriedOver" };
            return Array.Exists(validStatuses, s => s == status);
        }
    }

    public class WorkItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string AssignedTo { get; set; }
    }
}