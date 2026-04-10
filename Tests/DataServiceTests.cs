using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly string _testDataPath;

        public DataServiceTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _testDataPath = Path.Combine(Path.GetTempPath(), "test_data");
            Directory.CreateDirectory(_testDataPath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithValidJson_ReturnsProjectStatus()
        {
            var jsonContent = @"{
                ""milestones"": [
                    {
                        ""id"": ""m1"",
                        ""name"": ""Phase 1"",
                        ""targetDate"": ""2026-04-15T00:00:00Z"",
                        ""status"": ""OnTrack""
                    }
                ],
                ""tasks"": [
                    {
                        ""id"": ""t1"",
                        ""title"": ""Task 1"",
                        ""description"": ""Test task"",
                        ""status"": ""Completed"",
                        ""assignedTo"": ""John Doe"",
                        ""dueDate"": ""2026-04-10T00:00:00Z""
                    }
                ]
            }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, jsonContent);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            var result = await service.ReadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Single(result.Milestones);
            Assert.Single(result.Tasks);
            Assert.Equal("Phase 1", result.Milestones[0].Name);
            Assert.Equal("Task 1", result.Tasks[0].Title);

            File.Delete(filePath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            var dataPath = Path.Combine(_testDataPath, "missing");
            Directory.CreateDirectory(dataPath);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            await Assert.ThrowsAsync<FileNotFoundException>(() => service.ReadProjectDataAsync());
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithInvalidJson_ThrowsJsonException()
        {
            var invalidJson = "{ invalid json }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, invalidJson);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            await Assert.ThrowsAsync<JsonException>(() => service.ReadProjectDataAsync());

            File.Delete(filePath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithEmptyMilestones_ReturnsEmptyList()
        {
            var jsonContent = @"{
                ""milestones"": [],
                ""tasks"": []
            }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, jsonContent);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            var result = await service.ReadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Empty(result.Milestones);
            Assert.Empty(result.Tasks);

            File.Delete(filePath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithMultipleMilestones_DeserializesAllMilestones()
        {
            var jsonContent = @"{
                ""milestones"": [
                    {
                        ""id"": ""m1"",
                        ""name"": ""Milestone 1"",
                        ""targetDate"": ""2026-04-15T00:00:00Z"",
                        ""status"": ""OnTrack""
                    },
                    {
                        ""id"": ""m2"",
                        ""name"": ""Milestone 2"",
                        ""targetDate"": ""2026-05-15T00:00:00Z"",
                        ""status"": ""AtRisk""
                    },
                    {
                        ""id"": ""m3"",
                        ""name"": ""Milestone 3"",
                        ""targetDate"": ""2026-06-15T00:00:00Z"",
                        ""status"": ""Completed""
                    }
                ],
                ""tasks"": []
            }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, jsonContent);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            var result = await service.ReadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Milestones.Count);
            Assert.Equal("Milestone 1", result.Milestones[0].Name);
            Assert.Equal("Milestone 2", result.Milestones[1].Name);
            Assert.Equal("Milestone 3", result.Milestones[2].Name);

            File.Delete(filePath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithMultipleTasks_DeserializesAllTasks()
        {
            var jsonContent = @"{
                ""milestones"": [],
                ""tasks"": [
                    {
                        ""id"": ""t1"",
                        ""title"": ""Task 1"",
                        ""description"": ""First task"",
                        ""status"": ""Completed"",
                        ""assignedTo"": ""Alice"",
                        ""dueDate"": ""2026-04-10T00:00:00Z""
                    },
                    {
                        ""id"": ""t2"",
                        ""title"": ""Task 2"",
                        ""description"": ""Second task"",
                        ""status"": ""InProgress"",
                        ""assignedTo"": ""Bob"",
                        ""dueDate"": ""2026-04-20T00:00:00Z""
                    }
                ]
            }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, jsonContent);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            var result = await service.ReadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Tasks.Count);
            Assert.Equal("Task 1", result.Tasks[0].Title);
            Assert.Equal("Task 2", result.Tasks[1].Title);

            File.Delete(filePath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_SetsLastUpdatedTimestamp()
        {
            var jsonContent = @"{
                ""milestones"": [],
                ""tasks"": []
            }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, jsonContent);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            var beforeRead = DateTime.UtcNow;
            var result = await service.ReadProjectDataAsync();
            var afterRead = DateTime.UtcNow;

            Assert.NotNull(result);
            Assert.True(result.LastUpdated >= beforeRead && result.LastUpdated <= afterRead);

            File.Delete(filePath);
        }

        [Fact]
        public async System.Threading.Tasks.Task ReadProjectDataAsync_WithInvalidMilestoneStatus_ThrowsJsonException()
        {
            var jsonContent = @"{
                ""milestones"": [
                    {
                        ""id"": ""m1"",
                        ""name"": ""Invalid Milestone"",
                        ""targetDate"": """",
                        ""status"": ""InvalidStatus""
                    }
                ],
                ""tasks"": []
            }";

            var dataPath = Path.Combine(_testDataPath, "data");
            Directory.CreateDirectory(dataPath);
            var filePath = Path.Combine(dataPath, "data.json");
            await File.WriteAllTextAsync(filePath, jsonContent);

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(_testDataPath);
            var service = new DataService(_mockEnvironment.Object);

            await Assert.ThrowsAsync<JsonException>(() => service.ReadProjectDataAsync());

            File.Delete(filePath);
        }
    }
}