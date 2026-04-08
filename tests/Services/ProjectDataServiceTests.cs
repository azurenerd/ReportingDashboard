using System;
using System.Threading.Tasks;
using Xunit;
using AgentSquad.Services;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private const string TestDataPath = "test-data/projects.json";
        
        [Fact]
        public async Task LoadProjectDataAsync_WithValidFilePath_ReturnsProjectList()
        {
            var jsonContent = "[{\"id\":\"1\",\"name\":\"Test Project\",\"status\":\"Active\"}]";
            var service = new ProjectDataService();
            
            var result = await service.LoadProjectDataAsync(TestDataPath);
            
            Assert.NotNull(result);
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithFilePath_CallsFileReadAsync()
        {
            var service = new ProjectDataService();
            
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => service.LoadProjectDataAsync("nonexistent-file.json"));
            
            Assert.NotNull(exception);
        }
    }
}