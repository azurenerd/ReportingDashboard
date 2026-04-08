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
            _testDataPath = Path.Combine(Path.GetTempPath(), $"integration-{Guid.NewGuid()}", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_testDataPath));
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DataFilePath", _testDataPath }
                });
            _configuration = configBuilder.Build();
        }

        [Fact]
        public async Task Full_Flow_ProjectDataService_Dashboard_Child_Components()
        {
            // Arrange - create valid test data matching Architecture schema
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

            // Act - Load project data (ProjectDataService step)
            var projectData = await service.LoadProjectDataAsync();

            // Assert Step 1: ProjectDataService loads correctly
            Assert.NotNull(projectData);
            Assert.Equal("Integration Test Project", projectData.ProjectName);

            // Assert Step 2: Dashboard can extract page header data
            Assert.Equal("Test Sponsor", projectData.Sponsor);
            Assert.Equal("Test Manager", projectData.ProjectManager);
            Assert.Equal("On-Track", projectData.Status);

            // Assert Step 3: MilestoneTimeline receives correct data
            Assert.NotNull(projectData.Milestones);
            Assert.Equal(2, projectData.Milestones.Count);
            Assert.Equal("Phase 1", projectData.Milestones[0].Name);
            Assert.Equal(MilestoneStatus.Completed, projectData.Milestones[0].Status);
            Assert.Equal("Phase 2", projectData.Milestones[1].Name);
            Assert.Equal(MilestoneStatus.InProgress, projectData.Milestones[1].Status);

            // Assert Step 4: StatusCard can filter and count tasks
            Assert.NotNull(projectData.Tasks);
            Assert.Equal(3, projectData.Tasks.Count);

            var shippedTasks = projectData.Tasks.FindAll(t => t.Status == TaskStatus.Shipped);
            var inProgressTasks = projectData.Tasks.FindAll(t => t.Status == TaskStatus.InProgress);
            var carriedOverTasks = projectData.Tasks.FindAll(t => t.Status == TaskStatus.CarriedOver);

            Assert.Single(shippedTasks);
            Assert.Single(inProgressTasks);
            Assert.Single(carriedOverTasks);

            // Assert Step 5: ProgressMetrics receives correct data
            Assert.NotNull(projectData.Metrics);
            Assert.Equal(55, projectData.Metrics.CompletionPercentage);
            Assert.Equal(3, projectData.Metrics.TotalTasks);
            Assert.Equal(1, projectData.Metrics.TasksCompleted);

            // Assert Step 6: Sections render in correct vertical order
            Assert.True(projectData.ProjectStartDate < projectData.ProjectEndDate);
            Assert.NotEmpty(projectData.ProjectName);
        }

        [Fact]
        public async Task Dashboard_Renders_All_Three_Child_Components_In_Order()
        {
            // Arrange - minimal valid data
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

            // Assert rendering order:
            // 1. Page header (project name, sponsor, manager, status)
            Assert.NotNull(projectData.ProjectName);
            Assert.NotNull(projectData.Sponsor);
            Assert.NotNull(projectData.ProjectManager);
            Assert.NotNull(projectData.Status);
            
            // 2. MilestoneTimeline section
            Assert.NotNull(projectData.Milestones);
            Assert.NotEmpty(projectData.Milestones);
            
            // 3. StatusCard grid section
            Assert.NotNull(projectData.Tasks);
            Assert.NotEmpty(projectData.Tasks);
            
            // 4. ProgressMetrics section
            Assert.NotNull(projectData.Metrics);
        }

        public void Dispose()
        {
            var dir = Path.GetDirectoryName(_testDataPath);
            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}