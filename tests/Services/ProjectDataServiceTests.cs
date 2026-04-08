using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using AgentSquad.Services;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests : IDisposable
    {
        private readonly string _testDataDir;
        private readonly ProjectDataService _service;
        
        public ProjectDataServiceTests()
        {
            _testDataDir = Path.Combine(Path.GetTempPath(), $"agentsquad-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDataDir);
            _service = new ProjectDataService();
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithValidJsonFile_ReturnsDeserializedProjects()
        {
            var filePath = Path.Combine(_testDataDir, "valid-projects.json");
            var jsonContent = "[{\"id\":\"proj1\",\"name\":\"Test Project\",\"status\":\"Active\"}]";
            await File.WriteAllTextAsync(filePath, jsonContent);
            
            var result = await _service.LoadProjectDataAsync(filePath);
            
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Project", result[0].Name);
            Assert.Equal("Active", result[0].Status);
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithFilePath_ExecutesFileIoIntegration()
        {
            var filePath = Path.Combine(_testDataDir, "projects.json");
            var jsonContent = "[{\"id\":\"p1\",\"name\":\"Project\",\"status\":\"Pending\"}]";
            await File.WriteAllTextAsync(filePath, jsonContent);
            
            var result = await _service.LoadProjectDataAsync(filePath);
            
            Assert.NotNull(result);
            Assert.True(File.Exists(filePath));
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsDataLoadException()
        {
            var filePath = Path.Combine(_testDataDir, "nonexistent.json");
            
            await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(filePath));
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException()
        {
            var filePath = Path.Combine(_testDataDir, "malformed.json");
            await File.WriteAllTextAsync(filePath, "{ invalid json }");
            
            await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(filePath));
        }
        
        public void Dispose()
        {
            if (Directory.Exists(_testDataDir))
                Directory.Delete(_testDataDir, true);
        }
    }
}