using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AgentSquad.Dashboard.Services;
using AgentSquad.Dashboard.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.Tests.Unit.Components
{
    [Trait("Category", "Unit")]
    public class DashboardTests
    {
        private readonly Mock<ProjectDataService> _mockProjectDataService;
        private readonly Mock<ILogger<Dashboard>> _mockLogger;

        public DashboardTests()
        {
            _mockProjectDataService = new Mock<ProjectDataService>();
            _mockLogger = new Mock<ILogger<Dashboard>>();
        }

        [Fact]
        public void GetShippedCount_WithShippedTasks_ReturnsCorrectCount()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1", Status = TaskStatus.Shipped },
                new Task { Name = "Task2", Status = TaskStatus.Shipped },
                new Task { Name = "Task3", Status = TaskStatus.InProgress }
            };
            var dashboard = CreateDashboard(tasks);

            // Act
            var result = dashboard.GetShippedCount();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetShippedCount_WithNoShippedTasks_ReturnsZero()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1", Status = TaskStatus.InProgress }
            };
            var dashboard = CreateDashboard(tasks);

            // Act
            var result = dashboard.GetShippedCount();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetInProgressCount_WithInProgressTasks_ReturnsCorrectCount()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1", Status = TaskStatus.InProgress },
                new Task { Name = "Task2", Status = TaskStatus.InProgress },
                new Task { Name = "Task3", Status = TaskStatus.Shipped }
            };
            var dashboard = CreateDashboard(tasks);

            // Act
            var result = dashboard.GetInProgressCount();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetInProgressCount_WithNoInProgressTasks_ReturnsZero()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1", Status = TaskStatus.Shipped }
            };
            var dashboard = CreateDashboard(tasks);

            // Act
            var result = dashboard.GetInProgressCount();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetCarriedOverCount_WithCarriedOverTasks_ReturnsCorrectCount()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1", Status = TaskStatus.CarriedOver },
                new Task { Name = "Task2", Status = TaskStatus.CarriedOver }
            };
            var dashboard = CreateDashboard(tasks);

            // Act
            var result = dashboard.GetCarriedOverCount();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetCarriedOverCount_WithNoCarriedOverTasks_ReturnsZero()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1", Status = TaskStatus.Shipped }
            };
            var dashboard = CreateDashboard(tasks);

            // Act
            var result = dashboard.GetCarriedOverCount();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ProjectTitle_WithValidData_ReturnsProjectName()
        {
            // Arrange
            var projectData = new ProjectData
            {
                Project = new Project { Name = "Test Project" },
                Milestones = new List<Milestone>(),
                Tasks = new List<Task>(),
                Metrics = new ProjectMetrics()
            };
            var dashboard = CreateDashboard(projectData);

            // Act
            var result = dashboard.ProjectTitle;

            // Assert
            Assert.Equal("Test Project", result);
        }

        [Fact]
        public void ProjectTitle_WithoutData_ReturnsDefaultValue()
        {
            // Arrange
            var dashboard = CreateDashboardWithoutData();

            // Act
            var result = dashboard.ProjectTitle;

            // Assert
            Assert.Equal("Project Dashboard", result);
        }

        [Fact]
        public void ProjectDescription_WithValidData_ReturnsProjectDescription()
        {
            // Arrange
            var projectData = new ProjectData
            {
                Project = new Project { Description = "Test Description" },
                Milestones = new List<Milestone>(),
                Tasks = new List<Task>(),
                Metrics = new ProjectMetrics()
            };
            var dashboard = CreateDashboard(projectData);

            // Act
            var result = dashboard.ProjectDescription;

            // Assert
            Assert.Equal("Test Description", result);
        }

        [Fact]
        public void ProjectDescription_WithoutData_ReturnsDefaultValue()
        {
            // Arrange
            var dashboard = CreateDashboardWithoutData();

            // Act
            var result = dashboard.ProjectDescription;

            // Assert
            Assert.Equal("Loading...", result);
        }

        private Dashboard CreateDashboard(List<Task> tasks)
        {
            var projectData = new ProjectData
            {
                Project = new Project { Name = "Test" },
                Milestones = new List<Milestone>(),
                Tasks = tasks,
                Metrics = new ProjectMetrics()
            };
            return CreateDashboard(projectData);
        }

        private Dashboard CreateDashboard(ProjectData projectData)
        {
            var dashboard = new Dashboard
            {
                ProjectDataService = _mockProjectDataService.Object,
                Logger = _mockLogger.Object
            };
            // Set internal state through reflection or property
            return dashboard;
        }

        private Dashboard CreateDashboardWithoutData()
        {
            var dashboard = new Dashboard
            {
                ProjectDataService = _mockProjectDataService.Object,
                Logger = _mockLogger.Object
            };
            return dashboard;
        }
    }
}