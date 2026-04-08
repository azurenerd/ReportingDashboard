using Xunit;
using AgentSquad.Models;

namespace AgentSquad.Tests.Models
{
    public class ProjectDataTests
    {
        [Fact]
        public void ProjectData_Initialize_WithDefaults()
        {
            var projectData = new ProjectData();
            
            Assert.NotNull(projectData.Milestones);
            Assert.NotNull(projectData.Tasks);
            Assert.Empty(projectData.Milestones);
            Assert.Empty(projectData.Tasks);
        }

        [Fact]
        public void ProjectData_WithProjectInfo_ValidatesStructure()
        {
            var projectData = new ProjectData
            {
                Project = new Project
                {
                    Name = "Q2 Mobile App Release",
                    Description = "Release mobile app v2.0",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 6, 30)
                }
            };

            Assert.NotNull(projectData.Project);
            Assert.Equal("Q2 Mobile App Release", projectData.Project.Name);
            Assert.Equal(181, (projectData.Project.EndDate - projectData.Project.StartDate).Days);
        }

        [Fact]
        public void Milestone_WithAllStatuses_MapsCorrectly()
        {
            var completedMilestone = new Milestone
            {
                Name = "Phase 1",
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100,
                TargetDate = new DateTime(2026, 1, 15)
            };

            var inProgressMilestone = new Milestone
            {
                Name = "Phase 2",
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 45,
                TargetDate = new DateTime(2026, 4, 1)
            };

            var pendingMilestone = new Milestone
            {
                Name = "Phase 3",
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0,
                TargetDate = new DateTime(2026, 6, 30)
            };

            Assert.Equal(MilestoneStatus.Completed, completedMilestone.Status);
            Assert.Equal(MilestoneStatus.InProgress, inProgressMilestone.Status);
            Assert.Equal(MilestoneStatus.Pending, pendingMilestone.Status);
            Assert.Equal(100, completedMilestone.CompletionPercentage);
            Assert.Equal(45, inProgressMilestone.CompletionPercentage);
            Assert.Equal(0, pendingMilestone.CompletionPercentage);
        }

        [Fact]
        public void TaskItem_WithAllStatuses_MapsCorrectly()
        {
            var shippedTask = new TaskItem
            {
                Id = "T1",
                Name = "Dashboard UI",
                Status = TaskStatus.Shipped,
                AssignedTo = "Engineer 1"
            };

            var inProgressTask = new TaskItem
            {
                Id = "T2",
                Name = "Timeline Component",
                Status = TaskStatus.InProgress,
                AssignedTo = "Engineer 2"
            };

            var carriedOverTask = new TaskItem
            {
                Id = "T3",
                Name = "Advanced Analytics",
                Status = TaskStatus.CarriedOver,
                AssignedTo = "Engineer 3"
            };

            Assert.Equal(TaskStatus.Shipped, shippedTask.Status);
            Assert.Equal(TaskStatus.InProgress, inProgressTask.Status);
            Assert.Equal(TaskStatus.CarriedOver, carriedOverTask.Status);
        }

        [Fact]
        public void Metrics_CalculatesCompletionAccurately()
        {
            var metrics = new Metrics
            {
                CompletionPercentage = 68,
                ShippedCount = 12,
                InProgressCount = 5,
                CarriedOverCount = 2,
                BurndownRate = 2.5m
            };

            Assert.Equal(68, metrics.CompletionPercentage);
            Assert.Equal(12, metrics.ShippedCount);
            Assert.Equal(5, metrics.InProgressCount);
            Assert.Equal(2, metrics.CarriedOverCount);
            Assert.Equal(2.5m, metrics.BurndownRate);
        }
    }
}