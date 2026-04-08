using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using AgentSquad.Services;

namespace AgentSquad.Tests.Integration
{
    public class ProjectDataService_FileLoadingTests : IDisposable
    {
        private readonly string _testDataDir;
        private readonly ProjectDataService _service;
        
        public ProjectDataService_FileLoadingTests()
        {
            _testDataDir = Path.Combine(Path.GetTempPath(), $"agentsquad-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDataDir);
            _service = new ProjectDataService();
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithRealFile_ReturnsDeserializedData()
        {
            var filePath = Path.Combine(_testDataDir, "projects.json");
            var jsonContent = "[{\"id\":\"proj1\",\"name\":\"Test Project\",\"status\":\"Active\"}]";
            await File.WriteAllTextAsync(filePath, jsonContent);
            
            var result = await _service.LoadProjectDataAsync(filePath);
            
            Assert.Single(result);
            Assert.Equal("Test Project", result[0].Name);
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_MissingFile_ThrowsDataLoadException()
        {
            var filePath = Path.Combine(_testDataDir, "missing.json");
            
            await Assert.ThrowsAsync<DataLoadException>(
                () => _service.LoadProjectDataAsync(filePath));
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_MalformedJson_ThrowsDataLoadException()
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