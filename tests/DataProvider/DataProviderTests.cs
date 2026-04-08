using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AgentSquad.Runner.Tests.DataProvider
{
    public class DataProviderTests
    {
        private readonly string _testDataPath;
        private readonly string _testDataDirectory;

        public DataProviderTests()
        {
            _testDataDirectory = Path.Combine(Path.GetTempPath(), "dashboard-tests");
            _testDataPath = Path.Combine(_testDataDirectory, "data.json");
            Directory.CreateDirectory(_testDataDirectory);
        }

        private void CreateTestDataFile(string content)
        {
            File.WriteAllText(_testDataPath, content);
        }

        [Fact]
        public async Task LoadData_WithValidJsonFile_ReturnsProjectData()
        {
            // Arrange
            var validJson = @"{
                ""name"": ""Test Project"",
                ""description"": ""Test Description"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 45,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 12,
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(validJson);

            // Act
            var result = await LoadProjectDataAsync(_testDataPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Project", result.Name);
            Assert.Equal(45, result.CompletionPercentage);
            Assert.Equal("OnTrack", result.HealthStatus);
        }

        [Fact]
        public async Task LoadData_WithMissingRequiredField_ThrowsValidationException()
        {
            // Arrange - missing "name" field
            var invalidJson = @"{
                ""description"": ""No Name Project"",
                ""startDate"": ""2024-01-01"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""OnTrack"",
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(invalidJson);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => LoadProjectDataAsync(_testDataPath));
        }

        [Fact]
        public async Task LoadData_WithMalformedJson_ThrowsJsonException()
        {
            // Arrange
            var malformedJson = @"{ ""name"": ""Broken Json"", incomplete";
            CreateTestDataFile(malformedJson);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(
                () => LoadProjectDataAsync(_testDataPath));
        }

        [Fact]
        public async Task LoadData_WithMissingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDataDirectory, "nonexistent.json");

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => LoadProjectDataAsync(nonExistentPath));
        }

        [Fact]
        public async Task LoadData_WithInvalidHealthStatus_ThrowsValidationException()
        {
            // Arrange
            var invalidStatus = @"{
                ""name"": ""Test Project"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""InvalidStatus"",
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(invalidStatus);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => LoadProjectDataAsync(_testDataPath));
        }

        [Fact]
        public async Task LoadData_WithCompletionPercentageOutOfRange_ThrowsValidationException()
        {
            // Arrange - completionPercentage > 100
            var outOfRange = @"{
                ""name"": ""Test Project"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 150,
                ""healthStatus"": ""OnTrack"",
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(outOfRange);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => LoadProjectDataAsync(_testDataPath));
        }

        [Fact]
        public async Task LoadData_WithValidMilestones_LoadsCorrectly()
        {
            // Arrange
            var withMilestones = @"{
                ""name"": ""Project With Milestones"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 10,
                ""milestones"": [
                    {
                        ""name"": ""Phase 1: MVP Launch"",
                        ""targetDate"": ""2024-03-31"",
                        ""status"": ""Completed"",
                        ""description"": ""Core dashboard functionality""
                    },
                    {
                        ""name"": ""Phase 2: Expansion"",
                        ""targetDate"": ""2024-06-30"",
                        ""status"": ""InProgress"",
                        ""description"": ""Enhanced metrics""
                    }
                ],
                ""workItems"": []
            }";
            CreateTestDataFile(withMilestones);

            // Act
            var result = await LoadProjectDataAsync(_testDataPath);

            // Assert
            Assert.NotNull(result.Milestones);
            Assert.Equal(2, result.Milestones.Count);
            Assert.Equal("Phase 1: MVP Launch", result.Milestones[0].Name);
            Assert.Equal("Completed", result.Milestones[0].Status);
        }

        [Fact]
        public async Task LoadData_WithValidWorkItems_LoadsCorrectly()
        {
            // Arrange
            var withWorkItems = @"{
                ""name"": ""Project With Work Items"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 40,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 8,
                ""milestones"": [],
                ""workItems"": [
                    {
                        ""title"": ""API Integration"",
                        ""description"": ""Connect to data warehouse"",
                        ""status"": ""InProgress"",
                        ""assignedTo"": ""Team A""
                    },
                    {
                        ""title"": ""Reporting Module"",
                        ""description"": ""Export to PowerPoint"",
                        ""status"": ""Shipped"",
                        ""assignedTo"": ""Team B""
                    }
                ]
            }";
            CreateTestDataFile(withWorkItems);

            // Act
            var result = await LoadProjectDataAsync(_testDataPath);

            // Assert
            Assert.NotNull(result.WorkItems);
            Assert.Equal(2, result.WorkItems.Count);
            Assert.Equal("API Integration", result.WorkItems[0].Title);
            Assert.Equal("InProgress", result.WorkItems[0].Status);
        }

        [Fact]
        public async Task LoadData_WithNegativeCompletionPercentage_ThrowsValidationException()
        {
            // Arrange
            var negativePercentage = @"{
                ""name"": ""Test Project"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": -10,
                ""healthStatus"": ""OnTrack"",
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(negativePercentage);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => LoadProjectDataAsync(_testDataPath));
        }

        [Fact]
        public async Task LoadData_WithEmptyMilestonesList_LoadsSuccessfully()
        {
            // Arrange
            var emptyMilestones = @"{
                ""name"": ""Project No Milestones"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 0,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 0,
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(emptyMilestones);

            // Act
            var result = await LoadProjectDataAsync(_testDataPath);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Milestones);
        }

        [Fact]
        public async Task LoadData_WithValidDates_ParsesCorrectly()
        {
            // Arrange
            var validDates = @"{
                ""name"": ""Project With Dates"",
                ""startDate"": ""2024-01-15"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 25,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 5,
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(validDates);

            // Act
            var result = await LoadProjectDataAsync(_testDataPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2024, 1, 15), result.StartDate);
            Assert.Equal(new DateTime(2024, 12, 31), result.TargetEndDate);
        }

        [Fact]
        public async Task LoadData_WithInvalidDateFormat_ThrowsFormatException()
        {
            // Arrange
            var invalidDate = @"{
                ""name"": ""Project Bad Date"",
                ""startDate"": ""not-a-date"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 50,
                ""healthStatus"": ""OnTrack"",
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(invalidDate);

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(
                () => LoadProjectDataAsync(_testDataPath));
        }

        [Fact]
        public async Task LoadData_CachesDataInMemory_ReturnsIdenticalInstanceOnSecondCall()
        {
            // Arrange
            var validJson = @"{
                ""name"": ""Cached Project"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 30,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 7,
                ""milestones"": [],
                ""workItems"": []
            }";
            CreateTestDataFile(validJson);
            var provider = new DataProviderService(_testDataPath);

            // Act
            var firstCall = await provider.GetProjectDataAsync();
            var secondCall = await provider.GetProjectDataAsync();

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        private async Task<ProjectData> LoadProjectDataAsync(string path)
        {
            var provider = new DataProviderService(path);
            return await provider.GetProjectDataAsync();
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDataDirectory))
                {
                    Directory.Delete(_testDataDirectory, true);
                }
            }
            catch
            {
                // Cleanup best effort
            }
        }
    }

    public class ProjectData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetEndDate { get; set; }
        public int CompletionPercentage { get; set; }
        public string HealthStatus { get; set; }
        public int VelocityThisMonth { get; set; }
        public List<Milestone> Milestones { get; set; } = new();
        public List<WorkItem> WorkItems { get; set; } = new();
    }

    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }

    public class WorkItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string AssignedTo { get; set; }
    }

    public class DataProviderService
    {
        private readonly string _dataPath;
        private ProjectData _cachedData;

        public DataProviderService(string dataPath)
        {
            _dataPath = dataPath;
        }

        public async Task<ProjectData> GetProjectDataAsync()
        {
            if (_cachedData != null)
                return _cachedData;

            if (!File.Exists(_dataPath))
                throw new FileNotFoundException($"Data file not found: {_dataPath}");

            var json = await File.ReadAllTextAsync(_dataPath);
            _cachedData = JsonSerializer.Deserialize<ProjectData>(json) ?? throw new JsonException("Failed to deserialize data");

            ValidateProjectData(_cachedData);
            return _cachedData;
        }

        private void ValidateProjectData(ProjectData data)
        {
            if (string.IsNullOrEmpty(data.Name))
                throw new InvalidOperationException("Project name is required");

            if (data.CompletionPercentage < 0 || data.CompletionPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(data.CompletionPercentage), "Must be between 0 and 100");

            var validStatuses = new[] { "OnTrack", "AtRisk", "Blocked" };
            if (!validStatuses.Contains(data.HealthStatus))
                throw new ArgumentException($"Invalid health status: {data.HealthStatus}");
        }
    }
}