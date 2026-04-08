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
    public class ProjectDataServiceTests : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly string _testDataPath;

        public ProjectDataServiceTests()
        {
            _testDataPath = Path.Combine(Path.GetTempPath(), $"test-data-{Guid.NewGuid()}.json");
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DataFilePath", _testDataPath }
                });
            _configuration = configBuilder.Build();
        }

        [Fact]
        public async Task LoadProjectDataAsync_Throws_FileNotFoundException_When_File_Missing()
        {
            // Arrange
            var service = new ProjectDataService(_configuration);
            Assert.False(File.Exists(_testDataPath));

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => service.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_Throws_JsonException_With_Invalid_JSON()
        {
            // Arrange
            File.WriteAllText(_testDataPath, "{invalid json}");
            var service = new ProjectDataService(_configuration);

            // Act & Assert
            try
            {
                await Assert.ThrowsAsync<JsonException>(() => service.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Throws_ArgumentException_When_ProjectName_Empty()
        {
            // Arrange
            var invalidData = new
            {
                projectName = "",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new object[0],
                tasks = new object[0],
                metrics = new { completionPercentage = 0, totalTasks = 0, tasksCompleted = 0, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(invalidData));
            var service = new ProjectDataService(_configuration);

            // Act & Assert
            try
            {
                await Assert.ThrowsAsync<ArgumentException>(() => service.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Throws_ArgumentException_When_StartDate_After_EndDate()
        {
            // Arrange
            var invalidData = new
            {
                projectName = "Test",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-12-31",
                projectEndDate = "2026-01-01",
                status = "On-Track",
                milestones = new object[0],
                tasks = new object[0],
                metrics = new { completionPercentage = 0, totalTasks = 0, tasksCompleted = 0, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(invalidData));
            var service = new ProjectDataService(_configuration);

            // Act & Assert
            try
            {
                await Assert.ThrowsAsync<ArgumentException>(() => service.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Throws_ArgumentException_When_Completion_Percentage_Exceeds_100()
        {
            // Arrange
            var invalidData = new
            {
                projectName = "Test",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[] { new { name = "M1", targetDate = "2026-06-01", status = "Completed", completionPercentage = 150 } },
                tasks = new[] { new { id = 1, name = "T1", status = "Shipped", owner = "Owner" } },
                metrics = new { completionPercentage = 50, totalTasks = 1, tasksCompleted = 1, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(invalidData));
            var service = new ProjectDataService(_configuration);

            // Act & Assert
            try
            {
                await Assert.ThrowsAsync<ArgumentException>(() => service.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Succeeds_With_Valid_Data()
        {
            // Arrange
            var validData = new
            {
                projectName = "Valid Project",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[] { new { name = "M1", targetDate = "2026-06-01", status = "Completed", completionPercentage = 100 } },
                tasks = new[] { new { id = 1, name = "T1", status = "Shipped", owner = "Owner" } },
                metrics = new { completionPercentage = 100, totalTasks = 1, tasksCompleted = 1, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(validData));
            var service = new ProjectDataService(_configuration);

            // Act
            var result = await service.LoadProjectDataAsync();

            // Assert
            try
            {
                Assert.NotNull(result);
                Assert.Equal("Valid Project", result.ProjectName);
                Assert.Equal("Sponsor", result.Sponsor);
                Assert.Single(result.Milestones);
                Assert.Single(result.Tasks);
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Validates_Milestone_Dates_Within_Project_Range()
        {
            // Arrange
            var invalidData = new
            {
                projectName = "Test",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[] { new { name = "M1", targetDate = "2027-06-01", status = "Completed", completionPercentage = 100 } },
                tasks = new object[0],
                metrics = new { completionPercentage = 0, totalTasks = 0, tasksCompleted = 0, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(invalidData));
            var service = new ProjectDataService(_configuration);

            // Act & Assert
            try
            {
                await Assert.ThrowsAsync<ArgumentException>(() => service.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Validates_Task_Count_Sum_Matches_Total()
        {
            // Arrange - task sum (1+0+0=1) doesn't match totalTasks (5)
            var invalidData = new
            {
                projectName = "Test",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new object[0],
                tasks = new[] { new { id = 1, name = "T1", status = "Shipped", owner = "Owner" } },
                metrics = new { completionPercentage = 20, totalTasks = 5, tasksCompleted = 1, tasksInProgress = 0, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(invalidData));
            var service = new ProjectDataService(_configuration);

            // Act & Assert
            try
            {
                await Assert.ThrowsAsync<ArgumentException>(() => service.LoadProjectDataAsync());
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
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