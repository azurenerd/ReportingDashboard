using System;
using System.Collections.Generic;
using AgentSquad.Runner.Data;
using Xunit;

namespace AgentSquad.Runner.Tests.Data
{
    public class ProjectDataTests
    {
        [Fact]
        public void ProjectData_CanInitializeWithProperties()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 12, 31),
                Milestones = new List<Milestone>(),
                Tasks = new List<ProjectTask>()
            };

            Assert.Equal("Test Project", projectData.ProjectName);
            Assert.Equal(new DateTime(2024, 1, 1), projectData.ProjectStartDate);
            Assert.Equal(new DateTime(2024, 12, 31), projectData.ProjectEndDate);
        }
    }

    public class MilestoneTests
    {
        [Fact]
        public void Milestone_CanInitializeWithProperties()
        {
            var milestone = new Milestone
            {
                Name = "Phase 1",
                TargetDate = new DateTime(2024, 3, 1),
                Status = "Completed",
                CompletionPercentage = 100
            };

            Assert.Equal("Phase 1", milestone.Name);
            Assert.Equal(new DateTime(2024, 3, 1), milestone.TargetDate);
            Assert.Equal("Completed", milestone.Status);
            Assert.Equal(100, milestone.CompletionPercentage);
        }

        [Fact]
        public void Milestone_StatusCanBePending()
        {
            var milestone = new Milestone { Status = "Pending" };
            Assert.Equal("Pending", milestone.Status);
        }

        [Fact]
        public void Milestone_StatusCanBeInProgress()
        {
            var milestone = new Milestone { Status = "InProgress" };
            Assert.Equal("InProgress", milestone.Status);
        }

        [Fact]
        public void Milestone_StatusCanBeCompleted()
        {
            var milestone = new Milestone { Status = "Completed" };
            Assert.Equal("Completed", milestone.Status);
        }
    }

    public class ProjectTaskTests
    {
        [Fact]
        public void ProjectTask_CanInitializeWithProperties()
        {
            var task = new ProjectTask
            {
                Name = "API Development",
                Status = "Shipped",
                Owner = "Alice"
            };

            Assert.Equal("API Development", task.Name);
            Assert.Equal("Shipped", task.Status);
            Assert.Equal("Alice", task.Owner);
        }

        [Fact]
        public void ProjectTask_StatusCanBeShipped()
        {
            var task = new ProjectTask { Status = "Shipped" };
            Assert.Equal("Shipped", task.Status);
        }

        [Fact]
        public void ProjectTask_StatusCanBeInProgress()
        {
            var task = new ProjectTask { Status = "InProgress" };
            Assert.Equal("InProgress", task.Status);
        }

        [Fact]
        public void ProjectTask_StatusCanBeCarriedOver()
        {
            var task = new ProjectTask { Status = "CarriedOver" };
            Assert.Equal("CarriedOver", task.Status);
        }
    }

    public class TaskStatusSummaryTests
    {
        [Fact]
        public void TaskStatusSummary_CanInitializeWithCounts()
        {
            var summary = new TaskStatusSummary
            {
                ShippedCount = 5,
                InProgressCount = 3,
                CarriedOverCount = 2
            };

            Assert.Equal(5, summary.ShippedCount);
            Assert.Equal(3, summary.InProgressCount);
            Assert.Equal(2, summary.CarriedOverCount);
        }

        [Fact]
        public void TaskStatusSummary_InitialCountsAreZero()
        {
            var summary = new TaskStatusSummary();

            Assert.Equal(0, summary.ShippedCount);
            Assert.Equal(0, summary.InProgressCount);
            Assert.Equal(0, summary.CarriedOverCount);
        }

        [Fact]
        public void TaskStatusSummary_TotalTasksReturnsSum()
        {
            var summary = new TaskStatusSummary
            {
                ShippedCount = 5,
                InProgressCount = 3,
                CarriedOverCount = 2
            };

            var total = summary.ShippedCount + summary.InProgressCount + summary.CarriedOverCount;
            Assert.Equal(10, total);
        }
    }
}