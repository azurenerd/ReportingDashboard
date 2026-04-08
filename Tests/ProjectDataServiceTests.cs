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
    public class ProjectDataServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly string _testDataPath;

        public ProjectDataServiceTests()
        {
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DataFilePath", "test-data.json" }
                });
            _configuration = configBuilder.Build();
            _testDataPath = "test-data.json";
        }

        [Fact]
        public async Task LoadProjectDataAsync_Throws_FileNotFoundException_When_File_Missing()
        {
            // Arrange
            var service = new ProjectDataService(_configuration);

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
        public async Task LoadProjectDataAsync_Validates_Required_Fields()
        {
            // Arrange
            var invalidData = new { projectName = "" };
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
        public async Task LoadProjectDataAsync_Validates_Completion_Percentage_Bounds()
        {
            // Arrange
            var validData = new
            {
                projectName = "Test",
                sponsor = "Test",
                projectManager = "Test",
                projectStartDate = "2026-01-01",
                projectEndDate = "2026-12-31",
                status = "On-Track",
                milestones = new[] { new { name = "M1", targetDate = "2026-06-01", status = "Completed", completionPercentage = 150 } },
                tasks = new[] { new { id = 1, name = "T1", status = "Shipped", owner = "Owner" } },
                metrics = new { completionPercentage = 50, totalTasks = 1, tasksCompleted = 0, tasksInProgress = 1, tasksCarriedOver = 0 }
            };
            File.WriteAllText(_testDataPath, JsonSerializer.Serialize(validData));
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
    }
}