using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests
{
    public class ModelTests
    {
        [Fact]
        public void Project_CanBeInstantiated()
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = "Test Project",
                StartDate = DateTime.Now,
                TargetEndDate = DateTime.Now.AddMonths(3)
            };

            Assert.NotNull(project);
            Assert.Equal("Test Project", project.Name);
        }

        [Fact]
        public void Milestone_CanBeInstantiated()
        {
            var milestone = new Milestone
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Name = "Phase 1",
                ScheduledDate = DateTime.Now,
                Status = MilestoneStatus.Planned,
                CompletionPercentage = 0
            };

            Assert.NotNull(milestone);
            Assert.Equal("Phase 1", milestone.Name);
            Assert.Equal(MilestoneStatus.Planned, milestone.Status);
        }

        [Fact]
        public void WorkItem_CanBeInstantiated()
        {
            var workItem = new WorkItem
            {
                Id = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Title = "Test Task",
                Status = WorkItemStatus.New,
                CreatedDate = DateTime.Now
            };

            Assert.NotNull(workItem);
            Assert.Equal("Test Task", workItem.Title);
            Assert.Equal(WorkItemStatus.New, workItem.Status);
        }

        [Fact]
        public void DashboardMetrics_CanBeInstantiated()
        {
            var metrics = new DashboardMetrics
            {
                CompletionPercentage = 50,
                ShippedCount = 10,
                CarriedOverCount = 2,
                TotalWorkItems = 20,
                InProgressCount = 8,
                NewCount = 0
            };

            Assert.NotNull(metrics);
            Assert.Equal(50, metrics.CompletionPercentage);
            Assert.Equal(10, metrics.ShippedCount);
        }
    }
}