using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models.Tests
{
    public class NullSafetyTests
    {
        [Fact]
        public void Project_RequiredPropertiesCannotBeNull()
        {
            // Arrange
            var project = new Project();

            // Act & Assert
            Assert.NotNull(project.Milestones);
            Assert.NotNull(project.WorkItems);
            Assert.Empty(project.Milestones);
            Assert.Empty(project.WorkItems);
        }

        [Fact]
        public void Milestone_OptionalDescriptionCanBeNull()
        {
            // Arrange
            var milestone = new Milestone
            {
                Name = "Test",
                TargetDate = System.DateTime.Now,
                Status = MilestoneStatus.Future
            };

            // Act & Assert
            Assert.Null(milestone.Description);
        }

        [Fact]
        public void WorkItem_OptionalDescriptionCanBeNull()
        {
            // Arrange
            var workItem = new WorkItem
            {
                Title = "Test",
                Status = WorkItemStatus.InProgress
            };

            // Act & Assert
            Assert.Null(workItem.Description);
        }

        [Fact]
        public void WorkItem_OptionalAssignedToCanBeNull()
        {
            // Arrange
            var workItem = new WorkItem
            {
                Title = "Test",
                Status = WorkItemStatus.Shipped
            };

            // Act & Assert
            Assert.Null(workItem.AssignedTo);
        }

        [Fact]
        public void Project_OptionalDescriptionCanBeNull()
        {
            // Arrange
            var project = new Project { Name = "Test" };

            // Act & Assert
            Assert.Null(project.Description);
        }

        [Fact]
        public void ProjectMetrics_AllPropertiesAreNonNull()
        {
            // Arrange
            var metrics = new ProjectMetrics();

            // Act & Assert
            Assert.True(metrics.CompletionPercentage >= 0);
            Assert.NotNull(metrics.HealthStatus);
            Assert.True(metrics.VelocityThisMonth >= 0);
            Assert.True(metrics.TotalMilestones >= 0);
            Assert.True(metrics.CompletedMilestones >= 0);
        }
    }
}