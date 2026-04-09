using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AgentSquad.Dashboard.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.Tests.Unit.Services
{
    [Trait("Category", "Unit")]
    public class ProjectDataServiceTests
    {
        private readonly Mock<ILogger<ProjectDataService>> _mockLogger;
        private readonly ProjectDataService _service;

        public ProjectDataServiceTests()
        {
            _mockLogger = new Mock<ILogger<ProjectDataService>>();
            _service = new ProjectDataService(_mockLogger.Object);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_ReturnsProjectData()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.NotEmpty(result.Milestones);
            Assert.NotEmpty(result.Tasks);
            Assert.NotNull(result.Metrics);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_ReturnsCorrectProjectName()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(result.Project.Name);
            Assert.NotEmpty(result.Project.Name);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_ReturnsMilestonesOrdered()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            for (int i = 1; i < result.Milestones.Count; i++)
            {
                Assert.True(result.Milestones[i - 1].TargetDate <= result.Milestones[i].TargetDate);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_AllMilestonesHaveValidStatus()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            foreach (var milestone in result.Milestones)
            {
                Assert.True(Enum.IsDefined(typeof(MilestoneStatus), milestone.Status));
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_AllTasksHaveValidStatus()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            foreach (var task in result.Tasks)
            {
                Assert.True(Enum.IsDefined(typeof(TaskStatus), task.Status));
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_MetricsCompletionPercentageWithinRange()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            Assert.InRange(result.Metrics.CompletionPercentage, 0, 100);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_MetricsCompletedTasksLessThanTotal()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            Assert.True(result.Metrics.CompletedTasks <= result.Metrics.TotalTasks);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_BurndownRateNonNegative()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            Assert.True(result.Metrics.EstimatedBurndownRate >= 0);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_ProjectStartDateBeforeEndDate()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            Assert.True(result.Project.StartDate < result.Project.EndDate);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_AllTasksHaveNames()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            foreach (var task in result.Tasks)
            {
                Assert.NotNull(task.Name);
                Assert.NotEmpty(task.Name);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_AllMilestonesHaveNames()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            foreach (var milestone in result.Milestones)
            {
                Assert.NotNull(milestone.Name);
                Assert.NotEmpty(milestone.Name);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_MilestoneCompletionPercentageWithinRange()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            foreach (var milestone in result.Milestones)
            {
                Assert.InRange(milestone.CompletionPercentage, 0, 100);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_AllTasksHaveDueDates()
        {
            // Act
            var result = await _service.LoadProjectDataAsync();

            // Assert
            foreach (var task in result.Tasks)
            {
                Assert.True(task.DueDate != default(DateTime));
            }
        }
    }
}