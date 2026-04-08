using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly ProjectDataService _service;
        private readonly string _testDataPath;

        public ProjectDataServiceTests()
        {
            _service = new ProjectDataService();
            _testDataPath = Path.Combine(Path.GetTempPath(), "test_data.json");
        }

        private void CreateTestDataFile(object data)
        {
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_testDataPath, json);
        }

        private void Cleanup()
        {
            if (File.Exists(_testDataPath))
                File.Delete(_testDataPath);
        }

        [Fact]
        public async Task LoadProjectData_WithValidJson_ReturnsProjectData()
        {
            var testData = new
            {
                projectName = "Q2 Mobile App Release",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-06-30",
                milestones = new[]
                {
                    new { name = "Design Phase", targetDate = "2024-02-15", status = "Completed", completionPercentage = 100 }
                },
                tasks = new[]
                {
                    new { name = "API Development", status = "Shipped", owner = "Team A" }
                }
            };

            CreateTestDataFile(testData);

            var result = await _service.LoadProjectDataAsync(_testDataPath);

            Assert.NotNull(result);
            Assert.Equal("Q2 Mobile App Release", result.ProjectName);
            Assert.Equal(1, result.Milestones.Count);
            Cleanup();
        }

        [Fact]
        public async Task LoadProjectData_WithMalformedJson_ThrowsJsonException()
        {
            File.WriteAllText(_testDataPath, "{ invalid json }");

            await Assert.ThrowsAsync<JsonException>(() => _service.LoadProjectDataAsync(_testDataPath));

            Cleanup();
        }

        [Fact]
        public async Task LoadProjectData_WithMissingFile_ThrowsFileNotFoundException()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), "missing_file.json");

            await Assert.ThrowsAsync<FileNotFoundException>(() => _service.LoadProjectDataAsync(missingPath));
        }

        [Fact]
        public async Task LoadProjectData_WithEmptyJson_ThrowsJsonException()
        {
            File.WriteAllText(_testDataPath, "");

            await Assert.ThrowsAsync<JsonException>(() => _service.LoadProjectDataAsync(_testDataPath));

            Cleanup();
        }

        [Fact]
        public async Task LoadProjectData_WithValidData_ParsesMilestonesCorrectly()
        {
            var testData = new
            {
                projectName = "Test Project",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-12-31",
                milestones = new[]
                {
                    new { name = "Phase 1", targetDate = "2024-03-01", status = "Completed", completionPercentage = 100 },
                    new { name = "Phase 2", targetDate = "2024-06-01", status = "InProgress", completionPercentage = 50 },
                    new { name = "Phase 3", targetDate = "2024-09-01", status = "Pending", completionPercentage = 0 }
                },
                tasks = new Task[] { }
            };

            CreateTestDataFile(testData);

            var result = await _service.LoadProjectDataAsync(_testDataPath);

            Assert.Equal(3, result.Milestones.Count);
            Assert.Equal("Completed", result.Milestones[0].Status);
            Assert.Equal("InProgress", result.Milestones[1].Status);
            Assert.Equal("Pending", result.Milestones[2].Status);
            Cleanup();
        }

        [Fact]
        public async Task LoadProjectData_WithValidData_ParsesTasksCorrectly()
        {
            var testData = new
            {
                projectName = "Test Project",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-12-31",
                milestones = new object[] { },
                tasks = new[]
                {
                    new { name = "Task 1", status = "Shipped", owner = "Alice" },
                    new { name = "Task 2", status = "InProgress", owner = "Bob" },
                    new { name = "Task 3", status = "CarriedOver", owner = "Charlie" }
                }
            };

            CreateTestDataFile(testData);

            var result = await _service.LoadProjectDataAsync(_testDataPath);

            Assert.Equal(3, result.Tasks.Count);
            Assert.Equal("Shipped", result.Tasks[0].Status);
            Assert.Equal("InProgress", result.Tasks[1].Status);
            Assert.Equal("CarriedOver", result.Tasks[2].Status);
            Cleanup();
        }

        [Fact]
        public async Task GetTaskStatusSummary_WithMixedTasks_ReturnsCounts()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Name = "Task 1", Status = "Shipped", Owner = "Alice" },
                new ProjectTask { Name = "Task 2", Status = "Shipped", Owner = "Bob" },
                new ProjectTask { Name = "Task 3", Status = "InProgress", Owner = "Charlie" },
                new ProjectTask { Name = "Task 4", Status = "CarriedOver", Owner = "David" }
            };

            var summary = _service.GetTaskStatusSummary(tasks);

            Assert.Equal(2, summary.ShippedCount);
            Assert.Equal(1, summary.InProgressCount);
            Assert.Equal(1, summary.CarriedOverCount);
        }

        [Fact]
        public async Task GetTaskStatusSummary_WithNoTasks_ReturnsZeroCounts()
        {
            var tasks = new List<ProjectTask>();

            var summary = _service.GetTaskStatusSummary(tasks);

            Assert.Equal(0, summary.ShippedCount);
            Assert.Equal(0, summary.InProgressCount);
            Assert.Equal(0, summary.CarriedOverCount);
        }

        [Fact]
        public void CalculateCompletionPercentage_WithAllTasksShipped_Returns100()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Status = "Shipped" },
                new ProjectTask { Status = "Shipped" },
                new ProjectTask { Status = "Shipped" }
            };

            var percentage = _service.CalculateCompletionPercentage(tasks);

            Assert.Equal(100, percentage);
        }

        [Fact]
        public void CalculateCompletionPercentage_WithMixedTasks_CalculatesCorrectly()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Status = "Shipped" },
                new ProjectTask { Status = "Shipped" },
                new ProjectTask { Status = "InProgress" },
                new ProjectTask { Status = "CarriedOver" }
            };

            var percentage = _service.CalculateCompletionPercentage(tasks);

            Assert.Equal(50, percentage);
        }

        [Fact]
        public void CalculateCompletionPercentage_WithNoTasks_ReturnsZero()
        {
            var tasks = new List<ProjectTask>();

            var percentage = _service.CalculateCompletionPercentage(tasks);

            Assert.Equal(0, percentage);
        }
    }
}