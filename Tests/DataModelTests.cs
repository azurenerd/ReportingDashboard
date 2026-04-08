using Xunit;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests
{
    public class DataModelTests
    {
        [Fact]
        public void Project_Constructor_InitializesProperties()
        {
            // Arrange
            var id = "proj-001";
            var name = "Test Project";
            var description = "Test Description";
            var status = "Active";

            // Act
            var project = new Project
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
            var dueDate = new DateTime(2026, 06, 30);

            // Act
            var milestone = new Milestone
            {
                Id = id,
                Name = name,
                DueDate = dueDate
            };

            // Assert
            Assert.Equal(id, milestone.Id);
            Assert.Equal(name, milestone.Name);
            Assert.Equal(dueDate, milestone.DueDate);
        }

        [Fact]
        public void Task_Constructor_InitializesProperties()
        {
            // Arrange
            var id = "task-001";
            var title = "Create API";
            var status = "InProgress";

            // Act
            var task = new Task
            {
                Id = id,
                Title = title,
                Status = status
            };

            // Assert
            Assert.Equal(id, task.Id);
            Assert.Equal(title, task.Title);
            Assert.Equal(status, task.Status);
        }

        [Fact]
        public void Metrics_Constructor_InitializesProperties()
        {
            // Arrange
            var completionPercentage = 75;
            var tasksCompleted = 15;
            var totalTasks = 20;

            // Act
            var metrics = new Metrics
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
                Project = new Project { Id = "p1", Name = "Test" },
                Milestones = new List<Milestone>(),
                Tasks = new List<Task>(),
                Metrics = new Metrics()
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
        public void Milestone_WithNullDueDate_IsValid()
        {
            // Arrange & Act
            var milestone = new Milestone
            {
                Id = "mile-002",
                Name = "Future Phase",
                DueDate = null
            };

            // Assert
            Assert.Null(milestone.DueDate);
        }

        [Fact]
        public void Task_WithNullAssignee_IsValid()
        {
            // Arrange & Act
            var task = new Task
            {
                Id = "task-002",
                Title = "Flexible Task",
                Status = "Pending",
                Assignee = null
            };

            // Assert
            Assert.Null(task.Assignee);
        }
    }
}