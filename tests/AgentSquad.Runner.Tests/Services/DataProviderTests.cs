using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests
    {
        private const string TestDataDir = "wwwroot";
        private const string TestDataFile = "wwwroot/test-data.json";

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_ReturnsProject()
        {
            // Arrange
            var validJson = @"{
                ""name"": ""Test Project"",
                ""milestones"": [
                    {
                        ""id"": ""M1"",
                        ""title"": ""Milestone 1"",
                        ""status"": ""Active"",
                        ""completionPercentage"": 50,
                        ""workItems"": [
                            {
                                ""id"": ""W1"",
                                ""title"": ""Task 1"",
                                ""status"": ""InProgress"",
                                ""completionPercentage"": 75
                            }
                        ]
                    }
                ]
            }";
            
            System.IO.Directory.CreateDirectory(TestDataDir);
            System.IO.File.WriteAllText(TestDataFile, validJson);
            
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, TestDataFile);

            // Act
            var project = await provider.LoadProjectDataAsync();

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Test Project", project.Name);
            Assert.Single(project.Milestones);
            
            System.IO.File.Delete(TestDataFile);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            System.IO.Directory.CreateDirectory(TestDataDir);
            System.IO.File.WriteAllText(TestDataFile, invalidJson);
            
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, TestDataFile);

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
            
            System.IO.File.Delete(TestDataFile);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidCompletionPercentage_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidJson = @"{
                ""name"": ""Test"",
                ""milestones"": [
                    {
                        ""id"": ""M1"",
                        ""title"": ""Milestone"",
                        ""status"": ""Active"",
                        ""completionPercentage"": 150,
                        ""workItems"": []
                    }
                ]
            }";
            
            System.IO.Directory.CreateDirectory(TestDataDir);
            System.IO.File.WriteAllText(TestDataFile, invalidJson);
            
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, TestDataFile);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => provider.LoadProjectDataAsync());
            
            System.IO.File.Delete(TestDataFile);
        }

        [Fact]
        public async Task LoadProjectDataAsync_UsesCacheOnSecondCall()
        {
            // Arrange
            var validJson = @"{""name"": ""Test"", ""milestones"": []}";
            System.IO.Directory.CreateDirectory(TestDataDir);
            System.IO.File.WriteAllText(TestDataFile, validJson);
            
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, TestDataFile);

            // Act
            var first = await provider.LoadProjectDataAsync();
            var second = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Same(first, second);
            
            System.IO.File.Delete(TestDataFile);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, "wwwroot/nonexistent.json");

            // Act & Assert
            await Assert.ThrowsAsync<System.IO.FileNotFoundException>(() => provider.LoadProjectDataAsync());
        }
    }
}