using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using AgentSquad.Services;
using AgentSquad.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AgentSquad.Tests.Integration
{
    public class DashboardIntegrationTests
    {
        private readonly Mock<ILogger<DataProvider>> _mockLogger;
        private readonly MemoryCacheDataProvider _cacheProvider;
        private readonly Mock<IWebHostEnvironment> _mockHostEnvironment;
        private readonly DataProvider _dataProvider;

        public DashboardIntegrationTests()
        {
            _mockLogger = new Mock<ILogger<DataProvider>>();
            _cacheProvider = new MemoryCacheDataProvider();
            _mockHostEnvironment = new Mock<IWebHostEnvironment>();
            
            _mockHostEnvironment.Setup(h => h.ContentRootPath)
                .Returns(Path.GetTempPath());

            _dataProvider = new DataProvider(_mockLogger.Object, _cacheProvider, _mockHostEnvironment.Object);
        }

        [Fact]
        public async Task LoadAndCacheProjectData_FullWorkflow_Success()
        {
            var projectData = CreateTestProjectData();
            var tempFile = Path.Combine(Path.GetTempPath(), "integration_data.json");
            var json = System.Text.Json.JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                var projects = await _dataProvider.LoadProjectsAsync(tempFile);
                Assert.NotEmpty(projects);

                foreach (var project in projects)
                {
                    await _cacheProvider.SetAsync($"project_{project.Id}", project);
                }

                var cachedProject = await _cacheProvider.GetAsync<Project>($"project_{projects[0].Id}");
                Assert.NotNull(cachedProject);
                Assert.Equal(projects[0].Id, cachedProject.Id);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task RealWwwrootDataJsonIntegration_LoadsFromActualPath()
        {
            var projectData = CreateTestProjectData();
            var wwwrootDir = Path.Combine(Path.GetTempPath(), "wwwroot");
            var dataPath = Path.Combine(wwwrootDir, "data.json");
            Directory.CreateDirectory(wwwrootDir);

            _mockHostEnvironment.Setup(h => h.ContentRootPath)
                .Returns(Path.GetTempPath());

            var json = System.Text.Json.JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(dataPath, json);

            try
            {
                var projects = await _dataProvider.LoadProjectsAsync(dataPath);
                Assert.NotEmpty(projects);
                Assert.NotNull(projects[0].Id);
            }
            finally
            {
                if (File.Exists(dataPath)) File.Delete(dataPath);
                if (Directory.Exists(wwwrootDir)) Directory.Delete(wwwrootDir);
            }
        }

        [Fact]
        public async Task MultipleProjectsWithMetrics_LoadAndRetrieve_Success()
        {
            var projectData = new { projects = new List<object>() };
            for (int i = 0; i < 3; i++)
            {
                ((List<object>)projectData.projects).Add(new
                {
                    id = $"p{i}",
                    name = $"Project {i}",
                    startDate = "2024-01-01T00:00:00Z",
                    endDate = "2024-12-31T23:59:59Z",
                    milestones = new List<object>(),
                    workItems = new List<object>(),
                    metrics = new
                    {
                        velocityThisMonth = 10 + (i * 5),
                        healthStatus = "Healthy",
                        completionPercentage = 25 + (i * 25)
                    }
                });
            }

            var tempFile = Path.Combine(Path.GetTempPath(), "multi_project_data.json");
            var json = System.Text.Json.JsonSerializer.Serialize(projectData);
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                var projects = await _dataProvider.LoadProjectsAsync(tempFile);
                Assert.Equal(3, projects.Count);

                for (int i = 0; i < projects.Count; i++)
                {
                    await _cacheProvider.SetAsync($"project_{i}", projects[i]);
                }

                var retrieved = await _cacheProvider.GetAsync<Project>("project_0");
                Assert.NotNull(retrieved);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        private object CreateTestProjectData()
        {
            return new
            {
                projects = new List<object>
                {
                    new
                    {
                        id = "p1",
                        name = "Test Project",
                        startDate = "2024-01-01T00:00:00Z",
                        endDate = "2024-12-31T23:59:59Z",
                        milestones = new List<object>
                        {
                            new
                            {
                                id = "m1",
                                name = "Release v1.0",
                                status = "InProgress",
                                dueDate = "2024-06-30T23:59:59Z",
                                description = "Initial release"
                            }
                        },
                        workItems = new List<object>
                        {
                            new
                            {
                                id = "wi1",
                                title = "Implement core",
                                status = "InProgress",
                                milestoneId = "m1",
                                assignedTo = "user@example.com",
                                completionPercentage = 75
                            }
                        },
                        metrics = new
                        {
                            velocityThisMonth = 42,
                            healthStatus = "Healthy",
                            completionPercentage = 50
                        }
                    }
                }
            };
        }
    }
}