using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models
{
    public class WorkItemTests
    {
        [Fact]
        public void WorkItem_WithValidData_InitializesSuccessfully()
        {
            var workItem = new WorkItem
            {
                Title = "Implement authentication",
                Description = "Add JWT-based authentication",
                Status = WorkItemStatus.InProgress,
                AssignedTo = "Alice Smith"
            };

            Assert.Equal("Implement authentication", workItem.Title);
            Assert.Equal("Add JWT-based authentication", workItem.Description);
            Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
            Assert.Equal("Alice Smith", workItem.AssignedTo);
        }

        [Theory]
        [InlineData(WorkItemStatus.Shipped)]
        [InlineData(WorkItemStatus.InProgress)]
        [InlineData(WorkItemStatus.CarriedOver)]
        public void WorkItem_AcceptsAllStatuses(WorkItemStatus status)
        {
            var workItem = new WorkItem { Status = status };
            Assert.Equal(status, workItem.Status);
        }

        [Fact]
        public void WorkItem_AllowsNullAssignee()
        {
            var workItem = new WorkItem { AssignedTo = null };
            Assert.Null(workItem.AssignedTo);
        }

        [Fact]
        public void WorkItem_WithEmptyDescription_IsAllowed()
        {
            var workItem = new WorkItem { Description = string.Empty };
            Assert.Empty(workItem.Description);
        }
    }
}