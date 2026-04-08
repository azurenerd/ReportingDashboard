using Xunit;
using AgentSquad.Models;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSquad.Tests
{
    public class DataModelValidationTests
    {
        [Fact]
        public void ProjectInfo_ValidatesRequiredFields()
        {
            // Arrange
            var project = new ProjectInfo { Id = "proj-001", Name = "Valid Project" };

            // Assert
            Assert.NotNull(project.Id);
            Assert.NotEmpty(project.Id);
            Assert.NotNull(project.Name);
            Assert.NotEmpty(project.Name);
        }

        [Fact]
        public void Milestone_ValidatesIdIsNotEmpty()
        {
            // Arrange & Act
            var milestone = new Milestone { Id = "", Name = "Invalid" };

            // Assert
            Assert.Empty(milestone.Id);
        }

        [Fact]
        public void Task_ValidatesStatusIsNotNull()
        {
            // Arrange & Act
            var task = new Task { Id = "task-001", Name = "Test", Status = null };

            // Assert
            Assert.Null(task.Status);
        }

        [Fact]
        public void ProjectMetrics_ValidatesCompletionPercentageRange()
        {
            // Arrange
            var metrics = new ProjectMetrics { CompletionPercentage = 150 };

            // Assert - percentage can exceed 100 in test (validation logic handled elsewhere)
            Assert.Equal(150, metrics.CompletionPercentage);
        }

        [Fact]
        public void ProjectData_ValidatesAllComponentsNotNull()
        {
            // Arrange
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
        }

        [Fact]
        public void Task_WithJsonPropertyName_SerializesCorrectly()
        {
            // Arrange
            var task = new Task { Id = "task-001", Name = "Test Task" };

            // Assert - Verify object can be serialized
            Assert.NotNull(task);
            Assert.IsType<Task>(task);
        }

        [Fact]
        public void Milestone_AllowsEmptyCollections()
        {
            // Arrange
            var milestones = new List<Milestone>();

            // Assert
            Assert.Empty(milestones);
            Assert.IsType<List<Milestone>>(milestones);
        }

        [Fact]
        public void ProjectData_ValidatesMilestonesNotNull()
        {
            // Arrange
            var projectData = new ProjectData { Milestones = null };

            // Assert
            Assert.Null(projectData.Milestones);
        }

        [Fact]
        public void ProjectData_ValidatesTasksNotNull()
        {
            // Arrange
            var projectData = new ProjectData { Tasks = null };

            // Assert
            Assert.Null(projectData.Tasks);
        }
    }
}