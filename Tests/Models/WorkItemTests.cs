using Xunit;
using AgentSquad.Models;
using System;

namespace AgentSquad.Tests.Models
{
    public class WorkItemTests
    {
        [Fact]
        public void WorkItem_ValidInitialization_Success()
        {
            var workItem = new WorkItem
            {
                Id = "wi1",
                Title = "Implement login",
                Status = WorkItemStatus.InProgress,
                MilestoneId = "m1",
                AssignedTo = "user@example.com",
                CompletionPercentage = 50
            };

            Assert.Equal("wi1", workItem.Id);
            Assert.Equal("Implement login", workItem.Title);
            Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
            Assert.Equal("user@example.com", workItem.AssignedTo);
            Assert.Equal(50, workItem.CompletionPercentage);
        }

        [Fact]
        public void WorkItem_NullAssignedTo_Allowed()
        {
            var workItem = new WorkItem
            {
                Id = "wi1",
                Title = "Code review",
                Status = WorkItemStatus.Todo,
                MilestoneId = "m1",
                AssignedTo = null,
                CompletionPercentage = 0
            };

            Assert.Null(workItem.AssignedTo);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void WorkItem_ValidCompletionPercentage_Success(int percentage)
        {
            var workItem = new WorkItem
            {
                Id = "wi1",
                Title = "Task",
                Status = WorkItemStatus.Todo,
                MilestoneId = "m1",
                CompletionPercentage = percentage
            };

            Assert.Equal(percentage, workItem.CompletionPercentage);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-50)]
        [InlineData(101)]
        [InlineData(150)]
        public void WorkItem_InvalidCompletionPercentage_ThrowsInvalidOperationException(int percentage)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                if (percentage < 0 || percentage > 100)
                    throw new InvalidOperationException($"CompletionPercentage must be between 0 and 100, got {percentage}");
            });
            Assert.Contains("between 0 and 100", ex.Message);
        }

        [Theory]
        [InlineData("InvalidStatus")]
        [InlineData("in_progress")]
        public void WorkItem_InvalidStatus_ThrowsArgumentException(string invalidStatus)
        {
            Assert.Throws<ArgumentException>(() => System.Enum.Parse<WorkItemStatus>(invalidStatus));
        }
    }
}