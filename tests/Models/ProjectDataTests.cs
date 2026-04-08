using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models
{
    public class ProjectDataTests
    {
        [Fact]
        public void ProjectData_DefaultConstructor_InitializesCollections()
        {
            var projectData = new ProjectData();

            Assert.NotNull(projectData.Milestones);
            Assert.Empty(projectData.Milestones);
            Assert.NotNull(projectData.Tasks);
            Assert.Empty(projectData.Tasks);
        }

        [Fact]
        public void Milestone_WithAllProperties_StoresValues()
        {
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Phase 1",
                TargetDate = new DateTime(2024, 3, 31),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            };

            Assert.Equal("m1", milestone.Id);
            Assert.Equal("Phase 1", milestone.Name);
            Assert.Equal(new DateTime(2024, 3, 31), milestone.TargetDate);
            Assert.Equal(MilestoneStatus.Completed, milestone.Status);
            Assert.Equal(100, milestone.CompletionPercentage);
        }

        [Fact]
        public void Milestone_WithOptionalActualDate_StoresValue()
        {
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Phase 1",
                TargetDate = new DateTime(2024, 3, 31),
                ActualDate = new DateTime(2024, 3, 28),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            };

            Assert.NotNull(milestone.ActualDate);
            Assert.Equal(new DateTime(2024, 3, 28), milestone.ActualDate);
        }

        [Fact]
        public void ProjectTask_WithAllProperties_StoresValues()
        {
            var task = new ProjectTask
            {
                Id = "t1",
                Name = "Implement Feature",
                Status = TaskStatus.Shipped,
                AssignedTo = "Developer A",
                DueDate = new DateTime(2024, 2, 1),
                EstimatedDays = 5,
                RelatedMilestone = "Phase 1"
            };

            Assert.Equal("t1", task.Id);
            Assert.Equal("Implement Feature", task.Name);
            Assert.Equal(TaskStatus.Shipped, task.Status);
            Assert.Equal("Developer A", task.AssignedTo);
            Assert.Equal(new DateTime(2024, 2, 1), task.DueDate);
            Assert.Equal(5, task.EstimatedDays);
            Assert.Equal("Phase 1", task.RelatedMilestone);
        }

        [Fact]
        public void ProjectMetrics_WithAllProperties_StoresValues()
        {
            var metrics = new ProjectMetrics
            {
                TotalTasks = 10,
                CompletedTasks = 5,
                InProgressTasks = 3,
                CarriedOverTasks = 2,
                EstimatedBurndownRate = 0.20m
            };

            Assert.Equal(10, metrics.TotalTasks);
            Assert.Equal(5, metrics.CompletedTasks);
            Assert.Equal(3, metrics.InProgressTasks);
            Assert.Equal(2, metrics.CarriedOverTasks);
            Assert.Equal(0.20m, metrics.EstimatedBurndownRate);
        }

        [Fact]
        public void ProjectInfo_WithAllProperties_StoresValues()
        {
            var projectInfo = new ProjectInfo
            {
                Name = "Q2 Mobile App",
                Description = "Mobile app release",
                Status = "In Progress",
                Sponsor = "Executive Sponsor",
                ProjectManager = "PM Name",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31),
                CompletionPercentage = 50
            };

            Assert.Equal("Q2 Mobile App", projectInfo.Name);
            Assert.Equal("Mobile app release", projectInfo.Description);
            Assert.Equal("In Progress", projectInfo.Status);
            Assert.Equal("Executive Sponsor", projectInfo.Sponsor);
            Assert.Equal("PM Name", projectInfo.ProjectManager);
            Assert.Equal(new DateTime(2024, 1, 1), projectInfo.StartDate);
            Assert.Equal(new DateTime(2024, 12, 31), projectInfo.EndDate);
            Assert.Equal(50, projectInfo.CompletionPercentage);
        }

        [Fact]
        public void MilestoneStatus_Enum_HasExpectedValues()
        {
            Assert.Equal(0, (int)MilestoneStatus.Pending);
            Assert.Equal(1, (int)MilestoneStatus.InProgress);
            Assert.Equal(2, (int)MilestoneStatus.Completed);
        }

        [Fact]
        public void TaskStatus_Enum_HasExpectedValues()
        {
            Assert.Equal(0, (int)TaskStatus.Shipped);
            Assert.Equal(1, (int)TaskStatus.InProgress);
            Assert.Equal(2, (int)TaskStatus.CarriedOver);
        }
    }
}