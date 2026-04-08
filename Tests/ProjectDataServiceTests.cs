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
            _testDataPath = Path.Combine(Path.GetTempPath(), $"test-{Guid.NewGuid()}", "data.json");
            Directory.CreateDirectory(Path.GetDirectoryName(_testDataPath));
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
                Assert.Single(result.Milestones);
                Assert.Single(result.Tasks);
                Assert.Equal(TaskStatus.Shipped, result.Tasks[0].Status);
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_Enum_Deserialization()
        {
            // Arrange
            var validData = new
            {
                projectName = "Test",
                sponsor = "Sponsor",
                projectManager = "Manager",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[] { 
                    new { name = "M1", targetDate = "2026-06-01", status = "Completed", completionPercentage = 100 },
                    new { name = "M2", targetDate = "2026-09-01", status = "InProgress", completionPercentage = 50 }
                },
                tasks = new[] { 
                    new { id = 1, name = "T1", status = "Shipped", owner = "Owner1" },
                    new { id = 2, name = "T2", status = "InProgress", owner = "Owner2" }
                },
                metrics = new { completionPercentage = 50, totalTasks = 2, tasksCompleted = 1, tasksInProgress = 1, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(validData));
            var service = new ProjectDataService(_configuration);

            // Act
            var result = await service.LoadProjectDataAsync();

            // Assert
            try
            {
                Assert.Equal(MilestoneStatus.Completed, result.Milestones[0].Status);
                Assert.Equal(MilestoneStatus.InProgress, result.Milestones[1].Status);
                Assert.Equal(TaskStatus.Shipped, result.Tasks[0].Status);
                Assert.Equal(TaskStatus.InProgress, result.Tasks[1].Status);
            }
            finally
            {
                if (File.Exists(_testDataPath))
                    File.Delete(_testDataPath);
            }
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