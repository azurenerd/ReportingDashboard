using Xunit;
using AgentSquad.Runner.Data;
using System.Text.Json;

namespace AgentSquad.Runner.Tests.Data
{
    public class ProjectInfoTests
    {
        [Fact]
        public void ProjectInfo_Initialize_DefaultValues()
        {
            // Act
            var projectInfo = new ProjectInfo();

            // Assert
            Assert.Empty(projectInfo.Name);
            Assert.Empty(projectInfo.Description);
            Assert.Empty(projectInfo.Status);
            Assert.Empty(projectInfo.Sponsor);
            Assert.Empty(projectInfo.ProjectManager);
        }

        [Fact]
        public void ProjectInfo_SetProperties_ValuesAssigned()
        {
            // Arrange & Act
            var projectInfo = new ProjectInfo
            {
                Name = "Q2 Release",
                Description = "Mobile app v2.0",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 6, 30),
                Status = "OnTrack",
                Sponsor = "VP Engineering",
                ProjectManager = "John Doe"
            };

            // Assert
            Assert.Equal("Q2 Release", projectInfo.Name);
            Assert.Equal("Mobile app v2.0", projectInfo.Description);
            Assert.Equal("OnTrack", projectInfo.Status);
        }

        [Fact]
        public void ProjectInfo_DefaultStatus_IsOnTrack()
        {
            // Act
            var projectInfo = new ProjectInfo();

            // Assert
            Assert.Equal("OnTrack", projectInfo.Status);
        }
    }

    public class MilestoneTests
    {
        [Fact]
        public void Milestone_Initialize_DefaultValues()
        {
            // Act
            var milestone = new Milestone();

            // Assert
            Assert.Empty(milestone.Id);
            Assert.Empty(milestone.Name);
            Assert.Equal(MilestoneStatus.Pending, milestone.Status);
            Assert.Equal(0, milestone.CompletionPercentage);
            Assert.Null(milestone.ActualDate);
        }

        [Fact]
        public void Milestone_SetProperties_ValuesAssigned()
        {
            // Arrange
            var targetDate = new DateTime(2024, 3, 31);
            var actualDate = new DateTime(2024, 3, 28);

            // Act
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Design Complete",
                TargetDate = targetDate,
                ActualDate = actualDate,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            };

            // Assert
            Assert.Equal("m1", milestone.Id);
            Assert.Equal("Design Complete", milestone.Name);
            Assert.Equal(targetDate, milestone.TargetDate);
            Assert.Equal(actualDate, milestone.ActualDate);
            Assert.Equal(MilestoneStatus.Completed, milestone.Status);
            Assert.Equal(100, milestone.CompletionPercentage);
        }

        [Theory]
        [InlineData(MilestoneStatus.Completed, 100)]
        [InlineData(MilestoneStatus.InProgress, 50)]
        [InlineData(MilestoneStatus.Pending, 0)]
        public void Milestone_StatusVariations_Valid(MilestoneStatus status, int completionPercentage)
        {
            // Act
            var milestone = new Milestone
            {
                Status = status,
                CompletionPercentage = completionPercentage
            };

            // Assert
            Assert.Equal(status, milestone.Status);
            Assert.Equal(completionPercentage, milestone.CompletionPercentage);
        }

        [Fact]
        public void Milestone_CompletionPercentageEdgeCases_Valid()
        {
            // Act & Assert
            var milestone0 = new Milestone { CompletionPercentage = 0 };
            var milestone50 = new Milestone { CompletionPercentage = 50 };
            var milestone100 = new Milestone { CompletionPercentage = 100 };

            Assert.Equal(0, milestone0.CompletionPercentage);
            Assert.Equal(50, milestone50.CompletionPercentage);
            Assert.Equal(100, milestone100.CompletionPercentage);
        }
    }

    public class TaskTests
    {
        [Fact]
        public void Task_Initialize_DefaultValues()
        {
            // Act
            var task = new Task();

            // Assert
            Assert.Empty(task.Id);
            Assert.Empty(task.Name);
            Assert.Empty(task.AssignedTo);
            Assert.Empty(task.RelatedMilestone);
            Assert.Equal(0, task.EstimatedDays);
            Assert.Equal(TaskStatus.Shipped, task.Status);
        }

        [Fact]
        public void Task_SetProperties_ValuesAssigned()
        {
            // Arrange
            var dueDate = new DateTime(2024, 4, 15);

            // Act
            var task = new Task
            {
                Id = "t1",
                Name = "API Authentication",
                Status = TaskStatus.InProgress,
                AssignedTo = "Dev Team A",
                DueDate = dueDate,
                EstimatedDays = 5,
                RelatedMilestone = "m1"
            };

            // Assert
            Assert.Equal("t1", task.Id);
            Assert.Equal("API Authentication", task.Name);
            Assert.Equal(TaskStatus.InProgress, task.Status);
            Assert.Equal("Dev Team A", task.AssignedTo);
            Assert.Equal(dueDate, task.DueDate);
            Assert.Equal(5, task.EstimatedDays);
            Assert.Equal("m1", task.RelatedMilestone);
        }

        [Theory]
        [InlineData(TaskStatus.Shipped)]
        [InlineData(TaskStatus.InProgress)]
        [InlineData(TaskStatus.CarriedOver)]
        public void Task_StatusVariations_Valid(TaskStatus status)
        {
            // Act
            var task = new Task { Status = status };

            // Assert
            Assert.Equal(status, task.Status);
        }

        [Fact]
        public void Task_EstimatedDaysEdgeCases_Valid()
        {
            // Act & Assert
            var task1 = new Task { EstimatedDays = 1 };
            var task5 = new Task { EstimatedDays = 5 };
            var task30 = new Task { EstimatedDays = 30 };

            Assert.Equal(1, task1.EstimatedDays);
            Assert.Equal(5, task5.EstimatedDays);
            Assert.Equal(30, task30.EstimatedDays);
        }
    }

    public class ProjectMetricsTests
    {
        [Fact]
        public void ProjectMetrics_Initialize_DefaultValues()
        {
            // Act
            var metrics = new ProjectMetrics();

            // Assert
            Assert.Equal(0, metrics.TotalTasks);
            Assert.Equal(0, metrics.CompletedTasks);
            Assert.Equal(0, metrics.InProgressTasks);
            Assert.Equal(0, metrics.CarriedOverTasks);
            Assert.Equal(0, metrics.CompletionPercentage);
            Assert.Equal(0, metrics.EstimatedBurndownRate);
            Assert.Equal(0, metrics.DaysRemaining);
        }

        [Fact]
        public void ProjectMetrics_SetProperties_ValuesAssigned()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 6, 30);

            // Act
            var metrics = new ProjectMetrics
            {
                TotalTasks = 20,
                CompletedTasks = 10,
                InProgressTasks = 8,
                CarriedOverTasks = 2,
                CompletionPercentage = 50,
                EstimatedBurndownRate = 0.5,
                ProjectStartDate = startDate,
                ProjectEndDate = endDate,
                DaysRemaining = 100
            };

            // Assert
            Assert.Equal(20, metrics.TotalTasks);
            Assert.Equal(10, metrics.CompletedTasks);
            Assert.Equal(8, metrics.InProgressTasks);
            Assert.Equal(2, metrics.CarriedOverTasks);
            Assert.Equal(50, metrics.CompletionPercentage);
            Assert.Equal(0.5, metrics.EstimatedBurndownRate);
            Assert.Equal(100, metrics.DaysRemaining);
        }

        [Fact]
        public void ProjectMetrics_TaskCountsSum_Valid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics
            {
                TotalTasks = 20,
                CompletedTasks = 10,
                InProgressTasks = 8,
                CarriedOverTasks = 2
            };

            // Assert
            var sum = metrics.CompletedTasks + metrics.InProgressTasks + metrics.CarriedOverTasks;
            Assert.Equal(20, sum);
        }

        [Fact]
        public void ProjectMetrics_CompletionPercentageCalculation_Valid()
        {
            // Arrange & Act
            var metrics = new ProjectMetrics
            {
                TotalTasks = 10,
                CompletedTasks = 5
            };
            metrics.CompletionPercentage = (metrics.CompletedTasks * 100) / metrics.TotalTasks;

            // Assert
            Assert.Equal(50, metrics.CompletionPercentage);
        }

        [Fact]
        public void ProjectMetrics_BurndownRateVariations_Valid()
        {
            // Act & Assert
            var metricsSlow = new ProjectMetrics { EstimatedBurndownRate = 0.25 };
            var metricsMedium = new ProjectMetrics { EstimatedBurndownRate = 0.5 };
            var metricsFast = new ProjectMetrics { EstimatedBurndownRate = 1.0 };

            Assert.Equal(0.25, metricsSlow.EstimatedBurndownRate);
            Assert.Equal(0.5, metricsMedium.EstimatedBurndownRate);
            Assert.Equal(1.0, metricsFast.EstimatedBurndownRate);
        }
    }

    public class ProjectDataTests
    {
        [Fact]
        public void ProjectData_Initialize_DefaultCollections()
        {
            // Act
            var projectData = new ProjectData();

            // Assert
            Assert.NotNull(projectData.Milestones);
            Assert.NotNull(projectData.Tasks);
            Assert.Empty(projectData.Milestones);
            Assert.Empty(projectData.Tasks);
            Assert.Null(projectData.Project);
            Assert.Null(projectData.Metrics);
        }

        [Fact]
        public void ProjectData_SetProperties_ValuesAssigned()
        {
            // Arrange
            var project = new ProjectInfo { Name = "Test Project" };
            var milestones = new List<Milestone> 
            { 
                new Milestone { Id = "m1", Name = "Milestone 1" } 
            };
            var tasks = new List<Task> 
            { 
                new Task { Id = "t1", Name = "Task 1" } 
            };
            var metrics = new ProjectMetrics { TotalTasks = 1 };

            // Act
            var projectData = new ProjectData
            {
                Project = project,
                Milestones = milestones,
                Tasks = tasks,
                Metrics = metrics
            };

            // Assert
            Assert.NotNull(projectData.Project);
            Assert.Equal("Test Project", projectData.Project.Name);
            Assert.Single(projectData.Milestones);
            Assert.Single(projectData.Tasks);
            Assert.NotNull(projectData.Metrics);
        }

        [Fact]
        public void ProjectData_AddMultipleMilestones_CountsCorrect()
        {
            // Arrange & Act
            var projectData = new ProjectData();
            projectData.Milestones.Add(new Milestone { Id = "m1" });
            projectData.Milestones.Add(new Milestone { Id = "m2" });
            projectData.Milestones.Add(new Milestone { Id = "m3" });

            // Assert
            Assert.Equal(3, projectData.Milestones.Count);
        }

        [Fact]
        public void ProjectData_AddMultipleTasks_CountsCorrect()
        {
            // Arrange & Act
            var projectData = new ProjectData();
            projectData.Tasks.Add(new Task { Id = "t1" });
            projectData.Tasks.Add(new Task { Id = "t2" });

            // Assert
            Assert.Equal(2, projectData.Tasks.Count);
        }
    }

    public class MilestoneStatusEnumTests
    {
        [Fact]
        public void MilestoneStatus_Values_CorrectEnumValues()
        {
            // Assert
            Assert.Equal(0, (int)MilestoneStatus.Completed);
            Assert.Equal(1, (int)MilestoneStatus.InProgress);
            Assert.Equal(2, (int)MilestoneStatus.Pending);
        }
    }

    public class TaskStatusEnumTests
    {
        [Fact]
        public void TaskStatus_Values_CorrectEnumValues()
        {
            // Assert
            Assert.Equal(0, (int)TaskStatus.Shipped);
            Assert.Equal(1, (int)TaskStatus.InProgress);
            Assert.Equal(2, (int)TaskStatus.CarriedOver);
        }
    }
}