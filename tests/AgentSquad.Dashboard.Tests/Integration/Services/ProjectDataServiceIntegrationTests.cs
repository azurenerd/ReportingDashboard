using Xunit;
using Microsoft.Extensions.Logging;
using AgentSquad.Dashboard.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.Tests.Integration.Services
{
    [Trait("Category", "Integration")]
    public class ProjectDataServiceIntegrationTests : IAsyncLifetime
    {
        private readonly string _testDataDirectory;
        private readonly string _testDataFilePath;
        private readonly ILogger<ProjectDataService> _logger;
        private ProjectDataService _service;

        public ProjectDataServiceIntegrationTests()
        {
            _testDataDirectory = Path.Combine(Path.GetTempPath(), $"dashboard-tests-{Guid.NewGuid()}");
            _testDataFilePath = Path.Combine(_testDataDirectory, "data.json");
            _logger = CreateTestLogger();
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_testDataDirectory);
            await CreateValidTestDataFile();
            _service = new ProjectDataService(_logger);
        }

        public async Task DisposeAsync()
        {
            if (Directory.Exists(_testDataDirectory))
            {
                Directory.Delete(_testDataDirectory, recursive: true);
            }
            await Task.CompletedTask;
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidFile_ReturnsCompleteData()
        {
            var result = await _service.LoadProjectDataAsync();

            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.NotEmpty(result.Milestones);
            Assert.NotEmpty(result.Tasks);
            Assert.NotNull(result.Metrics);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidFile_ProjectHasRequiredFields()
        {
            var result = await _service.LoadProjectDataAsync();

            Assert.NotNull(result.Project.Name);
            Assert.NotEmpty(result.Project.Name);
            Assert.NotNull(result.Project.Description);
            Assert.True(result.Project.StartDate < result.Project.EndDate);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidFile_MilestonesAreOrdered()
        {
            var result = await _service.LoadProjectDataAsync();

            for (int i = 1; i < result.Milestones.Count; i++)
            {
                Assert.True(
                    result.Milestones[i - 1].TargetDate <= result.Milestones[i].TargetDate,
                    "Milestones must be ordered by target date");
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidFile_AllTasksHaveDates()
        {
            var result = await _service.LoadProjectDataAsync();

            foreach (var task in result.Tasks)
            {
                Assert.NotEqual(default(DateTime), task.DueDate);
                Assert.True(task.DueDate > DateTime.MinValue);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidFile_MetricsCalculatedCorrectly()
        {
            var result = await _service.LoadProjectDataAsync();

            var shippedCount = 0;
            foreach (var task in result.Tasks)
            {
                if (task.Status == TaskStatus.Shipped) shippedCount++;
            }

            Assert.Equal(shippedCount, result.Metrics.CompletedTasks);
            Assert.True(result.Metrics.CompletionPercentage >= 0 && result.Metrics.CompletionPercentage <= 100);
        }

        [Fact]
        public async Task LoadProjectDataAsync_MultipleConsecutiveCalls_ReturnsConsistentData()
        {
            var result1 = await _service.LoadProjectDataAsync();
            var result2 = await _service.LoadProjectDataAsync();

            Assert.Equal(result1.Project.Name, result2.Project.Name);
            Assert.Equal(result1.Milestones.Count, result2.Milestones.Count);
            Assert.Equal(result1.Tasks.Count, result2.Tasks.Count);
            Assert.Equal(result1.Metrics.CompletionPercentage, result2.Metrics.CompletionPercentage);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsException()
        {
            await File.WriteAllTextAsync(_testDataFilePath, "{ invalid json }");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithEmptyFile_ThrowsException()
        {
            await File.WriteAllTextAsync(_testDataFilePath, "");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsException()
        {
            File.Delete(_testDataFilePath);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullData_ThrowsException()
        {
            await File.WriteAllTextAsync(_testDataFilePath, "null");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadProjectDataAsync_AfterFileUpdate_ReturnsUpdatedData()
        {
            var result1 = await _service.LoadProjectDataAsync();
            var originalName = result1.Project.Name;

            var updatedData = CreateTestProjectData("Updated Project");
            await File.WriteAllTextAsync(
                _testDataFilePath,
                JsonSerializer.Serialize(updatedData, new JsonSerializerOptions { WriteIndented = true }));

            var result2 = await _service.LoadProjectDataAsync();

            Assert.NotEqual(originalName, result2.Project.Name);
            Assert.Equal("Updated Project", result2.Project.Name);
        }

        private async Task CreateValidTestDataFile()
        {
            var data = CreateTestProjectData("Q2 Mobile App Release");
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_testDataFilePath, json);
        }

        private TestProjectData CreateTestProjectData(string projectName)
        {
            var startDate = new DateTime(2024, 01, 01);
            var endDate = new DateTime(2024, 12, 31);

            return new TestProjectData
            {
                Project = new ProjectDto
                {
                    Name = projectName,
                    Description = "Test project description",
                    StartDate = startDate,
                    EndDate = endDate
                },
                Milestones = new List<MilestoneDto>
                {
                    new MilestoneDto
                    {
                        Name = "Phase 1",
                        TargetDate = new DateTime(2024, 03, 31),
                        Status = "Completed",
                        CompletionPercentage = 100
                    },
                    new MilestoneDto
                    {
                        Name = "Phase 2",
                        TargetDate = new DateTime(2024, 06, 30),
                        Status = "InProgress",
                        CompletionPercentage = 50
                    }
                },
                Tasks = new List<TaskDto>
                {
                    new TaskDto
                    {
                        Name = "Design UI",
                        Status = "Shipped",
                        AssignedTo = "Alice",
                        DueDate = new DateTime(2024, 02, 15)
                    },
                    new TaskDto
                    {
                        Name = "Implement Backend",
                        Status = "InProgress",
                        AssignedTo = "Bob",
                        DueDate = new DateTime(2024, 04, 15)
                    },
                    new TaskDto
                    {
                        Name = "Testing",
                        Status = "CarriedOver",
                        AssignedTo = "Charlie",
                        DueDate = new DateTime(2024, 05, 15)
                    }
                },
                Metrics = new MetricsDto
                {
                    CompletionPercentage = 33,
                    CompletedTasks = 1,
                    TotalTasks = 3,
                    EstimatedBurndownRate = 0.5
                }
            };
        }

        private ILogger<ProjectDataService> CreateTestLogger()
        {
            return new NullLogger<ProjectDataService>();
        }

        private class TestProjectData
        {
            public ProjectDto Project { get; set; }
            public List<MilestoneDto> Milestones { get; set; }
            public List<TaskDto> Tasks { get; set; }
            public MetricsDto Metrics { get; set; }
        }

        private class ProjectDto
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        private class MilestoneDto
        {
            public string Name { get; set; }
            public DateTime TargetDate { get; set; }
            public string Status { get; set; }
            public int CompletionPercentage { get; set; }
        }

        private class TaskDto
        {
            public string Name { get; set; }
            public string Status { get; set; }
            public string AssignedTo { get; set; }
            public DateTime DueDate { get; set; }
        }

        private class MetricsDto
        {
            public int CompletionPercentage { get; set; }
            public int CompletedTasks { get; set; }
            public int TotalTasks { get; set; }
            public double EstimatedBurndownRate { get; set; }
        }
    }
}