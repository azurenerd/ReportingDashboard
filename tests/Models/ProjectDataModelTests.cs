using Xunit;
using AgentSquad.Models;
using System;

namespace AgentSquad.Tests.Models
{
    public class ProjectDataModelTests
    {
        [Fact]
        public void ProjectData_CreateInstance_WithValidData()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                Status = "InProgress",
                CompletionPercentage = 50,
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 12, 31)
            };

            Assert.Equal("Test Project", projectData.ProjectName);
            Assert.Equal("InProgress", projectData.Status);
            Assert.Equal(50, projectData.CompletionPercentage);
        }

        [Fact]
        public void Task_CreateInstance_WithValidData()
        {
            var task = new Task
            {
                Title = "Sample Task",
                Description = "Task description",
                Status = "Pending",
                Priority = "High"
            };

            Assert.Equal("Sample Task", task.Title);
            Assert.Equal("Task description", task.Description);
            Assert.Equal("Pending", task.Status);
            Assert.Equal("High", task.Priority);
        }

        [Fact]
        public void ProjectData_CompletionPercentage_CanBeZero()
        {
            var projectData = new ProjectData
            {
                ProjectName = "New Project",
                Status = "NotStarted",
                CompletionPercentage = 0
            };

            Assert.Equal(0, projectData.CompletionPercentage);
        }

        [Fact]
        public void ProjectData_CompletionPercentage_CanBeHundred()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Completed Project",
                Status = "Completed",
                CompletionPercentage = 100
            };

            Assert.Equal(100, projectData.CompletionPercentage);
        }

        [Fact]
        public void Task_EmptyTitle_Allowed()
        {
            var task = new Task
            {
                Title = "",
                Status = "Pending"
            };

            Assert.Empty(task.Title);
        }
    }
}