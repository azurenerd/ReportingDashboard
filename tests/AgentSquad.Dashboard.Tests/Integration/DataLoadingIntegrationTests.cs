using Xunit;
using AgentSquad.Dashboard.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class DataLoadingIntegrationTests : IAsyncLifetime
    {
        private readonly string _testDirectory;
        private readonly string _dataFilePath;
        private ProjectDataService _service;

        public DataLoadingIntegrationTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"dashboard-loading-{Guid.NewGuid()}");
            _dataFilePath = Path.Combine(_testDirectory, "data.json");
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_testDirectory);
            _service = new ProjectDataService();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            await Task.CompletedTask;
        }

        [Fact]
        public async Task LoadData_WithCompleteJsonStructure_PopulatesAllFields()
        {
            var data = CreateCompleteProjectData();
            await SaveTestData(data);

            var result = await _service.LoadProjectDataAsync();

            Assert.NotNull(result.Project);
            Assert.NotEmpty(result.Milestones);
            Assert.NotEmpty(result.Tasks);
            Assert.NotNull(result.Metrics);
        }

        [Fact]
        public async Task LoadData_WithPartialData_HandlesGracefully()
        {
            var partialData = new { Project = new { Name = "Test" } };
            await File.WriteAllTextAsync(_dataFilePath, JsonSerializer.Serialize(partialData));

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_WithLargeDataset_LoadsSuccessfully()
        {
            var data = CreateLargeProjectData();
            await SaveTestData(data);

            var result = await _service.LoadProjectDataAsync();

            Assert.NotEmpty(result.Tasks);
            Assert.True(result.Tasks.Count >= 50);
        }

        [Fact]
        public async Task LoadData_WithZeroMetrics_LoadsSuccessfully()
        {
            var data = new
            {
                Project = new { Name = "Test", Description = "", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) },
                Milestones = new List<object>(),
                Tasks = new List<object>(),
                Metrics = new { CompletionPercentage = 0, CompletedTasks = 0, TotalTasks = 0, EstimatedBurndownRate = 0.0 }
            };
            await SaveTestData(data);

            var result = await _service.LoadProjectDataAsync();

            Assert.Equal(0, result.Metrics.CompletionPercentage);
        }

        [Fact]
        public async Task LoadData_WithMaxValues_LoadsSuccessfully()
        {
            var data = new
            {
                Project = new { Name = "Test", Description = "Desc", StartDate = DateTime.MinValue.AddYears(1), EndDate = DateTime.MaxValue.AddYears(-1) },
                Milestones = new List<object>(),
                Tasks = new List<object>(),
                Metrics = new { CompletionPercentage = 100, CompletedTasks = 1000, TotalTasks = 1000, EstimatedBurndownRate = 999.99 }
            };
            await SaveTestData(data);

            var result = await _service.LoadProjectDataAsync();

            Assert.Equal(100, result.Metrics.CompletionPercentage);
        }

        [Fact]
        public async Task LoadData_WithSpecialCharactersInNames_PreservesContent()
        {
            var data = new
            {
                Project = new { Name = "Test™ Project® 2024", Description = "Special: <>&\"'", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1) },
                Milestones = new List<object>(),
                Tasks = new List<object>(),
                Metrics = new { CompletionPercentage = 0, CompletedTasks = 0, TotalTasks = 0, EstimatedBurndownRate = 0.0 }
            };
            await SaveTestData(data);

            var result = await _service.LoadProjectDataAsync();

            Assert.Contains("™", result.Project.Name);
        }

        private object CreateCompleteProjectData()
        {
            return new
            {
                Project = new { Name = "Complete Project", Description = "Full data", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
                Milestones = new List<object>
                {
                    new { Name = "M1", TargetDate = DateTime.Now.AddDays(10), Status = "Completed", CompletionPercentage = 100 }
                },
                Tasks = new List<object>
                {
                    new { Name = "T1", Status = "Shipped", AssignedTo = "User", DueDate = DateTime.Now.AddDays(5) }
                },
                Metrics = new { CompletionPercentage = 100, CompletedTasks = 1, TotalTasks = 1, EstimatedBurndownRate = 1.0 }
            };
        }

        private object CreateLargeProjectData()
        {
            var tasks = new List<object>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(new { Name = $"Task{i}", Status = "Shipped", AssignedTo = $"User{i}", DueDate = DateTime.Now.AddDays(i) });
            }

            return new
            {
                Project = new { Name = "Large Project", Description = "Many tasks", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(100) },
                Milestones = new List<object>(),
                Tasks = tasks,
                Metrics = new { CompletionPercentage = 50, CompletedTasks = 50, TotalTasks = 100, EstimatedBurndownRate = 1.0 }
            };
        }

        private async Task SaveTestData(object data)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
    }
}