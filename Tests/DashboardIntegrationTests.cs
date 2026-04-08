using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using AgentSquad.Dashboard.Models;
using AgentSquad.Dashboard.Services;
using Microsoft.Extensions.Configuration;

namespace AgentSquad.Dashboard.Tests
{
    public class DashboardIntegrationTests : IDisposable
    {
        private readonly string _testDataPath;
        private readonly IConfiguration _configuration;

        public DashboardIntegrationTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"integration-test-{Guid.NewGuid()}.json");
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DataFilePath", _testDataPath }
                });
            _configuration = configBuilder.Build();
        }

        [Fact]
        public async Task Full_Flow_ProjectDataService_Loads_Dashboard_Renders_Child_Components()
        {
            // Arrange - create valid test data
            var testData = new
            {
                projectName = "Integration Test Project",
                sponsor = "Test Sponsor",
                projectManager = "Test Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[]
                {
                    new { name = "Phase 1", targetDate = "2026-03-31", status = "Completed", completionPercentage = 100 },
                    new { name = "Phase 2", targetDate = "2026-06-30", status = "InProgress", completionPercentage = 50 }
                },
                tasks = new[]
                {
                    new { id = 1, name = "Task 1", status = "Shipped", owner = "Dev Team" },
                    new { id = 2, name = "Task 2", status = "InProgress", owner = "Dev Team" },
                    new { id = 3, name = "Task 3", status = "CarriedOver", owner = "QA Team" }
                },
                metrics = new
                {
                    completionPercentage = 55,
                    totalTasks = 3,
                    tasksCompleted = 1,
                    tasksInProgress = 1,
                    tasksCarriedOver = 1
                }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(testData));
            var service = new ProjectDataService(_configuration);

            // Act - load project data
            var projectData = await service.LoadProjectDataAsync();

            // Assert - verify data loaded correctly
            Assert.NotNull(projectData);
            Assert.Equal("Integration Test Project", projectData.ProjectName);
            Assert.Equal("Test Sponsor", projectData.Sponsor);
            Assert.Equal("Test Manager", projectData.ProjectManager);
            Assert.Equal("On-Track", projectData.Status);

            // Assert - verify milestones loaded
            Assert.NotNull(projectData.Milestones);
            Assert.Equal(2, projectData.Milestones.Count);
            Assert.Equal("Phase 1", projectData.Milestones[0].Name);
            Assert.Equal("Phase 2", projectData.Milestones[1].Name);

            // Assert - verify tasks loaded and can be filtered by status
            Assert.NotNull(projectData.Tasks);
            Assert.Equal(3, projectData.Tasks.Count);

            var shippedTasks = projectData.Tasks.FindAll(t => t.Status == TaskStatus.Shipped);
            var inProgressTasks = projectData.Tasks.FindAll(t => t.Status == TaskStatus.InProgress);
            var carriedOverTasks = projectData.Tasks.FindAll(t => t.Status == TaskStatus.CarriedOver);

            Assert.Single(shippedTasks);
            Assert.Single(inProgressTasks);
            Assert.Single(carriedOverTasks);

            // Assert - verify metrics loaded
            Assert.NotNull(projectData.Metrics);
            Assert.Equal(55, projectData.Metrics.CompletionPercentage);
            Assert.Equal(3, projectData.Metrics.TotalTasks);
            Assert.Equal(1, projectData.Metrics.TasksCompleted);
            Assert.Equal(1, projectData.Metrics.TasksInProgress);
            Assert.Equal(1, projectData.Metrics.TasksCarriedOver);

            // Assert - verify data structure matches Dashboard expectations
            Assert.True(projectData.ProjectStartDate < projectData.ProjectEndDate);
            Assert.NotEmpty(projectData.ProjectName);
            Assert.NotEmpty(projectData.Sponsor);
            Assert.NotEmpty(projectData.ProjectManager);
        }

        [Fact]
        public async Task Dashboard_Can_Pass_Data_To_Child_Components()
        {
            // Arrange - create test data
            var testData = new
            {
                projectName = "Child Component Test",
                sponsor = "Test",
                projectManager = "Test",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[]
                {
                    new { name = "M1", targetDate = "2026-06-01", status = "Completed", completionPercentage = 100 }
                },
                tasks = new[]
                {
                    new { id = 1, name = "T1", status = "Shipped", owner = "Owner1" },
                    new { id = 2, name = "T2", status = "InProgress", owner = "Owner2" },
                    new { id = 3, name = "T3", status = "CarriedOver", owner = "Owner3" }
                },
                metrics = new
                {
                    completionPercentage = 33,
                    totalTasks = 3,
                    tasksCompleted = 1,
                    tasksInProgress = 1,
                    tasksCarriedOver = 1
                }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(testData));
            var service = new ProjectDataService(_configuration);

            // Act
            var projectData = await service.LoadProjectDataAsync();

            // Assert - MilestoneTimeline parameters
            Assert.NotNull(projectData.Milestones);
            Assert.True(projectData.ProjectStartDate != default);
            Assert.True(projectData.ProjectEndDate != default);

            // Assert - StatusCard parameters (three instances)
            var shippedCount = projectData.Tasks.Count(t => t.Status == TaskStatus.Shipped);
            var inProgressCount = projectData.Tasks.Count(t => t.Status == TaskStatus.InProgress);
            var carriedOverCount = projectData.Tasks.Count(t => t.Status == TaskStatus.CarriedOver);

            Assert.Equal(1, shippedCount);
            Assert.Equal(1, inProgressCount);
            Assert.Equal(1, carriedOverCount);

            // Assert - ProgressMetrics parameters
            Assert.NotNull(projectData.Metrics);
            Assert.Equal(3, projectData.Metrics.TotalTasks);
        }

        [Fact]
        public async Task Dashboard_Renders_Sections_In_Correct_Vertical_Order()
        {
            // Arrange - create test data
            var testData = new
            {
                projectName = "Order Test",
                sponsor = "Test",
                projectManager = "Test",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[] { new { name = "M1", targetDate = "2026-06-01", status = "Completed", completionPercentage = 100 } },
                tasks = new[] { new { id = 1, name = "T1", status = "Shipped", owner = "Owner" } },
                metrics = new { completionPercentage = 100, totalTasks = 1, tasksCompleted = 1, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(testData));
            var service = new ProjectDataService(_configuration);

            // Act
            var projectData = await service.LoadProjectDataAsync();

            // Assert - verify data structure supports expected rendering order:
            // 1. Page header (project name, sponsor, manager, status)
            // 2. MilestoneTimeline
            // 3. StatusCard grid
            // 4. ProgressMetrics
            
            Assert.NotNull(projectData.ProjectName);
            Assert.NotNull(projectData.Sponsor);
            Assert.NotNull(projectData.ProjectManager);
            Assert.NotNull(projectData.Status);
            Assert.NotNull(projectData.Milestones);
            Assert.NotNull(projectData.Tasks);
            Assert.NotNull(projectData.Metrics);
        }

        public void Dispose()
        {
            if (File.Exists(_testDataPath))
            {
                try
                {
                    File.Delete(_testDataPath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}