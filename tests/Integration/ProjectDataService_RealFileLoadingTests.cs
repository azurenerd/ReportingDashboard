using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using AgentSquad.Services;

namespace AgentSquad.Tests.Integration
{
    public class ProjectDataService_RealFileLoadingTests : IDisposable
    {
        private readonly string _testDataDir;
        private readonly ProjectDataService _service;
        
        public ProjectDataService_RealFileLoadingTests()
        {
            _testDataDir = Path.Combine(Path.GetTempPath(), $"agentsquad-tests-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDataDir);
            _service = new ProjectDataService(new DefaultFileProvider());
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_LoadsRealJsonFile()
        {
            var filePath = Path.Combine(_testDataDir, "projects.json");
            var jsonContent = "[{\"id\":\"1\",\"name\":\"Real Project\",\"status\":\"Active\"}]";
            await File.WriteAllTextAsync(filePath, jsonContent);
            
            var result = await _service.LoadProjectDataAsync(filePath);
            
            Assert.Single(result);
            Assert.Equal("Real Project", result[0].Name);
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsDataLoadException()
        {
            var filePath = Path.Combine(_testDataDir, "nonexistent.json");
            
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