using Xunit;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests
{
    public class DataModelTests
    {
        [Fact]
        public void ProjectInfo_Constructor_InitializesProperties()
        {
            // Arrange
            var id = "proj-001";
            var name = "Test Project";
            var description = "Test Description";
            var status = "Active";

            // Act
            var project = new ProjectInfo
            {
                Id = id,
                Name = name,
                Description = description,
                Status = status
            };

            // Assert
            Assert.Equal(id, project.Id);
            Assert.Equal(name, project.Name);
            Assert.Equal(description, project.Description);
            Assert.Equal(status, project.Status);
        }

        [Fact]
        public void Milestone_Constructor_InitializesProperties()
        {
            // Arrange
            var id = "mile-001";
            var name = "Phase 1";
            var targetDate = new DateTime(2026, 06, 30);

            // Act
            var milestone = new Milestone
            {
                Id = id,
                Name = name,
                TargetDate = targetDate
            };

            // Assert
            Assert.Equal(id, milestone.Id);
            Assert.Equal(name, milestone.Name);
            Assert.Equal(targetDate, milestone.TargetDate);
        }

        [Fact]
        public void Task_Constructor_InitializesProperties()
        {
            // Arrange
            var id = "task-001";
            var name = "Create API";
            var status = "InProgress";

            // Act
            var task = new Task
            {
                Id = id,
                Name = name,
                Status = status
            };

            // Assert
            Assert.Equal(id, task.Id);
            Assert.Equal(name, task.Name);
            Assert.Equal(status, task.Status);
        }

        [Fact]
        public void ProjectMetrics_Constructor_InitializesProperties()
        {
            // Arrange
            var completionPercentage = 75;
            var tasksCompleted = 15;
            var totalTasks = 20;

            // Act
            var metrics = new ProjectMetrics
            {
                CompletionPercentage = completionPercentage,
                TasksCompleted = tasksCompleted,
                TotalTasks = totalTasks
            };

            // Assert
            Assert.Equal(completionPercentage, metrics.CompletionPercentage);
            Assert.Equal(tasksCompleted, metrics.TasksCompleted);
            Assert.Equal(totalTasks, metrics.TotalTasks);
        }

        [Fact]
        public void ProjectData_Constructor_InitializesWithCollections()
        {
            // Arrange & Act
            var projectData = new ProjectData
            {
                Project = new ProjectInfo { Id = "p1", Name = "Test" },
                Milestones = new List<Milestone>(),
                Tasks = new List<Task>(),
                Metrics = new ProjectMetrics()
            };

            // Assert
            Assert.NotNull(projectData.Project);
            Assert.NotNull(projectData.Milestones);
            Assert.NotNull(projectData.Tasks);
            Assert.NotNull(projectData.Metrics);
            Assert.Empty(projectData.Milestones);
            Assert.Empty(projectData.Tasks);
        }

        [Fact]
        public void Milestone_WithNullTargetDate_IsValid()
        {
            // Arrange & Act
            var milestone = new Milestone
            {
                Id = "mile-002",
                Name = "Future Phase",
                TargetDate = null
            };

            // Assert
            Assert.Null(milestone.TargetDate);
        }

        [Fact]
        public void Task_WithNullAssignee_IsValid()
        {
            // Arrange & Act
            var task = new Task
            {
                Id = "task-002",
                Name = "Flexible Task",
                Status = "Pending",
                AssignedTo = null
            };

            // Assert
            Assert.Null(task.AssignedTo);
        }
    }
}