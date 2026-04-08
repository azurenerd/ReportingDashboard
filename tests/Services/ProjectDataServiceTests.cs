using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private const string TestDataPath = "test-data/valid-projects.json";
        
        [Fact]
        public async Task LoadProjectDataAsync_WithValidFilePath_ReturnsProjectList()
        {
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(f => f.ReadAllTextAsync(TestDataPath))
                .ReturnsAsync("[{\"id\":\"1\",\"name\":\"Test Project\"}]");
            
            var service = new ProjectDataService(mockFileProvider.Object);
            var result = await service.LoadProjectDataAsync(TestDataPath);
            
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Project", result[0].Name);
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsDataLoadException()
        {
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException());
            
            var service = new ProjectDataService(mockFileProvider.Object);
            
            await Assert.ThrowsAsync<DataLoadException>(
                () => service.LoadProjectDataAsync("nonexistent.json"));
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_ThrowsDataLoadException()
        {
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(f => f.ReadAllTextAsync(TestDataPath))
                .ReturnsAsync("{invalid json}");
            
            var service = new ProjectDataService(mockFileProvider.Object);
            
            await Assert.ThrowsAsync<DataLoadException>(
                () => service.LoadProjectDataAsync(TestDataPath));
        }
    }
}