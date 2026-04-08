using Xunit;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests
{
    public class DataModelEdgeCaseTests
    {
        [Fact]
        public void Project_WithEmptyStringId_IsAllowed()
        {
            // Arrange & Act
            var project = new Project { Id = "", Name = "Test" };

            // Assert
            Assert.Empty(project.Id);
        }

        [Fact]
        public void Project_WithSpecialCharactersInName_IsAllowed()
        {
            // Arrange
            var name = "Project @#$% 2026";

            // Act
            var project = new Project { Id = "p1", Name = name };

            // Assert
            Assert.Equal(name, project.Name);
        }

        [Fact]
        public void Milestone_WithVeryLongName_IsAllowed()
        {
            // Arrange
            var longName = new string('A', 1000);

            // Act
            var milestone = new Milestone { Id = "m1", Name = longName };

            // Assert
            Assert.Equal(longName, milestone.Name);
        }

        [Fact]
        public void Milestone_WithPastDueDate_IsAllowed()
        {
            // Arrange
            var pastDate = DateTime.Now.AddYears(-1);

            // Act
            var milestone = new Milestone { Id = "m1", Name = "Past", DueDate = pastDate };

            // Assert
            Assert.True(milestone.DueDate < DateTime.Now);
        }

        [Fact]
        public void Task_WithZeroDuration_IsAllowed()
        {
            // Arrange & Act
            var task = new Task
            {
                Id = "t1",
                Title = "Zero Duration",
                EstimatedHours = 0
            };

            // Assert
            Assert.Equal(0, task.EstimatedHours);
        }

        [Fact]
        public void Task_WithNegativeProgress_IsAllowed()
        {
            // Arrange & Act
            var task = new Task
            {
                Id = "t1",
                Title = "Test",
                ProgressPercentage = -10
            };

            // Assert
            Assert.Equal(-10, task.ProgressPercentage);
        }

        [Fact]
        public void Metrics_WithZeroValues_IsAllowed()
        {
            // Arrange & Act
            var metrics = new Metrics
            {
                CompletionPercentage = 0,
                TasksCompleted = 0,
                TotalTasks = 0
            };

            // Assert
            Assert.Equal(0, metrics.CompletionPercentage);
            Assert.Equal(0, metrics.TasksCompleted);
            Assert.Equal(0, metrics.TotalTasks);
        }

        [Fact]
        public void ProjectData_WithEmptyMilestonesList_IsValid()
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
            Assert.Empty(projectData.Milestones);
        }

        [Fact]
        public void ProjectData_WithEmptyTasksList_IsValid()
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
            Assert.Empty(projectData.Tasks);
        }

        [Fact]
        public void ProjectData_WithLargeCollections_IsAllowed()
        {
            // Arrange
            var milestones = new List<Milestone>();
            var tasks = new List<Task>();
            for (int i = 0; i < 10000; i++)
            {
                milestones.Add(new Milestone { Id = $"m{i}", Name = $"Milestone {i}" });
                tasks.Add(new Task { Id = $"t{i}", Title = $"Task {i}" });
            }

            // Act
            var projectData = new ProjectData
            {
                Project = new Project { Id = "p1", Name = "Large Project" },
                Milestones = milestones,
                Tasks = tasks,
                Metrics = new Metrics()
            };

            // Assert
            Assert.Equal(10000, projectData.Milestones.Count);
            Assert.Equal(10000, projectData.Tasks.Count);
        }
    }
}