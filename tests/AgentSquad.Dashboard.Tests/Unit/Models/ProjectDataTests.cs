using Xunit;
using AgentSquad.Dashboard.Services;
using System;
using System.Collections.Generic;

namespace AgentSquad.Dashboard.Tests.Unit.Models
{
    [Trait("Category", "Unit")]
    public class ProjectDataTests
    {
        [Fact]
        public void ProjectData_WithValidData_CanBeCreated()
        {
            // Arrange & Act
            var data = new ProjectData();

            // Assert
            Assert.NotNull(data);
        }

        [Fact]
        public void ProjectData_Project_CanBeSet()
        {
            // Arrange
            var project = new Project { Name = "Test" };
            var data = new ProjectData { Project = project };

            // Act
            var result = data.Project;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public void ProjectData_Milestones_CanBeSet()
        {
            // Arrange
            var milestones = new List<Milestone> { new Milestone { Name = "M1" } };
            var data = new ProjectData { Milestones = milestones };

            // Act
            var result = data.Milestones;

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void ProjectData_Tasks_CanBeSet()
        {
            // Arrange
            var tasks = new List<Task> { new Task { Name = "T1" } };
            var data = new ProjectData { Tasks = tasks };

            // Act
            var result = data.Tasks;

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void ProjectData_Metrics_CanBeSet()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = 50 };
            var data = new ProjectData { Metrics = metrics };

            // Act
            var result = data.Metrics;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result.CompletionPercentage);
        }
    }

    [Trait("Category", "Unit")]
    public class ProjectTests
    {
        [Fact]
        public void Project_Name_CanBeSet()
        {
            // Arrange
            var project = new Project { Name = "MyProject" };

            // Act
            var result = project.Name;

            // Assert
            Assert.Equal("MyProject", result);
        }

        [Fact]
        public void Project_Description_CanBeSet()
        {
            // Arrange
            var project = new Project { Description = "Test Description" };

            // Act
            var result = project.Description;

            // Assert
            Assert.Equal("Test Description", result);
        }

        [Fact]
        public void Project_StartDate_CanBeSet()
        {
            // Arrange
            var date = new DateTime(2024, 01, 01);
            var project = new Project { StartDate = date };

            // Act
            var result = project.StartDate;

            // Assert
            Assert.Equal(date, result);
        }

        [Fact]
        public void Project_EndDate_CanBeSet()
        {
            // Arrange
            var date = new DateTime(2024, 12, 31);
            var project = new Project { EndDate = date };

            // Act
            var result = project.EndDate;

            // Assert
            Assert.Equal(date, result);
        }
    }

    [Trait("Category", "Unit")]
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_Name_CanBeSet()
        {
            // Arrange
            var milestone = new Milestone { Name = "Phase1" };

            // Act
            var result = milestone.Name;

            // Assert
            Assert.Equal("Phase1", result);
        }

        [Fact]
        public void Milestone_TargetDate_CanBeSet()
        {
            // Arrange
            var date = new DateTime(2024, 06, 01);
            var milestone = new Milestone { TargetDate = date };

            // Act
            var result = milestone.TargetDate;

            // Assert
            Assert.Equal(date, result);
        }

        [Fact]
        public void Milestone_ActualDate_CanBeSetToNull()
        {
            // Arrange
            var milestone = new Milestone { ActualDate = null };

            // Act
            var result = milestone.ActualDate;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Milestone_ActualDate_CanBeSet()
        {
            // Arrange
            var date = new DateTime(2024, 06, 05);
            var milestone = new Milestone { ActualDate = date };

            // Act
            var result = milestone.ActualDate;

            // Assert
            Assert.Equal(date, result);
        }

        [Fact]
        public void Milestone_Status_CanBeSet()
        {
            // Arrange
            var milestone = new Milestone { Status = MilestoneStatus.Completed };

            // Act
            var result = milestone.Status;

            // Assert
            Assert.Equal(MilestoneStatus.Completed, result);
        }

        [Fact]
        public void Milestone_CompletionPercentage_CanBeSet()
        {
            // Arrange
            var milestone = new Milestone { CompletionPercentage = 75 };

            // Act
            var result = milestone.CompletionPercentage;

            // Assert
            Assert.Equal(75, result);
        }
    }

    [Trait("Category", "Unit")]
    public class TaskTests
    {
        [Fact]
        public void Task_Name_CanBeSet()
        {
            // Arrange
            var task = new Task { Name = "Implement Feature" };

            // Act
            var result = task.Name;

            // Assert
            Assert.Equal("Implement Feature", result);
        }

        [Fact]
        public void Task_Status_CanBeSet()
        {
            // Arrange
            var task = new Task { Status = TaskStatus.Shipped };

            // Act
            var result = task.Status;

            // Assert
            Assert.Equal(TaskStatus.Shipped, result);
        }

        [Fact]
        public void Task_AssignedTo_CanBeSet()
        {
            // Arrange
            var task = new Task { AssignedTo = "John Doe" };

            // Act
            var result = task.AssignedTo;

            // Assert
            Assert.Equal("John Doe", result);
        }

        [Fact]
        public void Task_DueDate_CanBeSet()
        {
            // Arrange
            var date = new DateTime(2024, 03, 15);
            var task = new Task { DueDate = date };

            // Act
            var result = task.DueDate;

            // Assert
            Assert.Equal(date, result);
        }
    }

    [Trait("Category", "Unit")]
    public class ProjectMetricsTests
    {
        [Fact]
        public void ProjectMetrics_CompletionPercentage_CanBeSet()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = 65 };

            // Act
            var result = metrics.CompletionPercentage;

            // Assert
            Assert.Equal(65, result);
        }

        [Fact]
        public void ProjectMetrics_CompletedTasks_CanBeSet()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletedTasks = 6 };

            // Act
            var result = metrics.CompletedTasks;

            // Assert
            Assert.Equal(6, result);
        }

        [Fact]
        public void ProjectMetrics_TotalTasks_CanBeSet()
        {
            // Arrange
            var metrics = new ProjectMetrics { TotalTasks = 10 };

            // Act
            var result = metrics.TotalTasks;

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void ProjectMetrics_EstimatedBurndownRate_CanBeSet()
        {
            // Arrange
            var metrics = new ProjectMetrics { EstimatedBurndownRate = 1.5 };

            // Act
            var result = metrics.EstimatedBurndownRate;

            // Assert
            Assert.Equal(1.5, result);
        }
    }
}