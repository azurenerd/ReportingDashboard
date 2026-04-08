using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests
    {
        private readonly Mock<IDataCache> _mockCache;
        private readonly Mock<ILogger<DataProvider>> _mockLogger;
        private readonly DataProvider _dataProvider;

        public DataProviderTests()
        {
            _mockCache = new Mock<IDataCache>();
            _mockLogger = new Mock<ILogger<DataProvider>>();
            _dataProvider = new DataProvider(_mockCache.Object, _mockLogger.Object);
        }

        private Project CreateValidProject()
        {
            return new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                StartDate = DateTime.Now,
                TargetEndDate = DateTime.Now.AddMonths(3),
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                Milestones = new List<Milestone>
                {
                    new Milestone
                    {
                        Name = "Phase 1",
                        TargetDate = DateTime.Now.AddMonths(1),
                        Status = MilestoneStatus.Completed,
                        Description = "Phase 1 milestone"
                    }
                },
                WorkItems = new List<WorkItem>
                {
                    new WorkItem
                    {
                        Title = "Task 1",
                        Description = "First task",
                        Status = WorkItemStatus.Shipped,
                        AssignedTo = "Developer A"
                    }
                }
            };
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCacheHit_ReturnsCachedData()
        {
            var cachedProject = CreateValidProject();
            _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                .ReturnsAsync(cachedProject);

            var result = await _dataProvider.LoadProjectDataAsync();

            Assert.Equal(cachedProject, result);
            _mockCache.Verify(c => c.GetAsync<Project>("project_data"), Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJsonFile_LoadsAndCachesData()
        {
            var validJson = JsonSerializer.Serialize(CreateValidProject());
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, validJson);

                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);
                _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan>()))
                    .Returns(Task.CompletedTask);

                var result = await _dataProvider.LoadProjectDataAsync();

                Assert.NotNull(result);
                _mockCache.Verify(c => c.SetAsync("project_data", It.IsAny<Project>(), TimeSpan.FromHours(1)), Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                .ReturnsAsync((Project)null);

            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                _dataProvider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsInvalidOperationException()
        {
            var invalidJson = "{ invalid json }";
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, invalidJson);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void InvalidateCache_CallsRemoveOnCache()
        {
            _dataProvider.InvalidateCache();

            _mockCache.Verify(c => c.Remove("project_data"), Times.Once);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNullProjectData_ThrowsInvalidOperationException()
        {
            var nullProjectJson = "null";
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, nullProjectJson);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingProjectName_ThrowsInvalidOperationException()
        {
            var project = CreateValidProject();
            project.Name = "";
            var json = JsonSerializer.Serialize(project);
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, json);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithNoMilestones_ThrowsInvalidOperationException()
        {
            var project = CreateValidProject();
            project.Milestones = new List<Milestone>();
            var json = JsonSerializer.Serialize(project);
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, json);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidHealthStatus_ThrowsInvalidOperationException()
        {
            var invalidJson = @"{
                ""name"": ""Test Project"",
                ""description"": ""Test"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""InvalidStatus"",
                ""velocityThisMonth"": 5,
                ""milestones"": [{
                    ""name"": ""Phase 1"",
                    ""targetDate"": ""2024-03-31"",
                    ""status"": ""Completed"",
                    ""description"": ""Phase 1""
                }],
                ""workItems"": []
            }";

            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, invalidJson);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidMilestoneStatus_ThrowsInvalidOperationException()
        {
            var invalidJson = @"{
                ""name"": ""Test Project"",
                ""description"": ""Test"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 5,
                ""milestones"": [{
                    ""name"": ""Phase 1"",
                    ""targetDate"": ""2024-03-31"",
                    ""status"": ""InvalidMilestoneStatus"",
                    ""description"": ""Phase 1""
                }],
                ""workItems"": []
            }";

            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, invalidJson);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidWorkItemStatus_ThrowsInvalidOperationException()
        {
            var invalidJson = @"{
                ""name"": ""Test Project"",
                ""description"": ""Test"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 5,
                ""milestones"": [{
                    ""name"": ""Phase 1"",
                    ""targetDate"": ""2024-03-31"",
                    ""status"": ""Completed"",
                    ""description"": ""Phase 1""
                }],
                ""workItems"": [{
                    ""title"": ""Task"",
                    ""description"": ""Task description"",
                    ""status"": ""InvalidWorkItemStatus"",
                    ""assignedTo"": ""Developer A""
                }]
            }";

            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, invalidJson);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCompletionPercentageBelowZero_ThrowsInvalidOperationException()
        {
            var project = CreateValidProject();
            project.CompletionPercentage = -1;
            var json = JsonSerializer.Serialize(project);
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, json);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithCompletionPercentageAboveHundred_ThrowsInvalidOperationException()
        {
            var project = CreateValidProject();
            project.CompletionPercentage = 101;
            var json = JsonSerializer.Serialize(project);
            var tempFile = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(tempFile, json);
                _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                    .ReturnsAsync((Project)null);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    _dataProvider.LoadProjectDataAsync());
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidCompletionPercentages_Succeeds()
        {
            foreach (var percentage in new[] { 0, 1, 50, 99, 100 })
            {
                var project = CreateValidProject();
                project.CompletionPercentage = percentage;
                var json = JsonSerializer.Serialize(project);
                var tempFile = Path.GetTempFileName();

                try
                {
                    await File.WriteAllTextAsync(tempFile, json);
                    _mockCache.Setup(c => c.GetAsync<Project>("project_data"))
                        .ReturnsAsync((Project)null);
                    _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<TimeSpan>()))
                        .Returns(Task.CompletedTask);

                    var result = await _dataProvider.LoadProjectDataAsync();

                    Assert.NotNull(result);
                    Assert.Equal(percentage, result.CompletionPercentage);
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}