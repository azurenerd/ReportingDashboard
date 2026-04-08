using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AgentSquad.Services;

namespace AgentSquad.Tests.Integration
{
    public class ProjectDataService_ErrorHandlingTests
    {
        [Fact]
        public async Task LoadProjectDataAsync_MalformedJson_ThrowsDataLoadException()
        {
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
                .ReturnsAsync("{ invalid: json }");
            
            var service = new ProjectDataService(mockFileProvider.Object);
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => service.LoadProjectDataAsync("test.json"));
            
            Assert.Contains("parse", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public async Task LoadProjectDataAsync_FileNotFound_ThrowsDataLoadException()
        {
            var mockFileProvider = new Mock<IFileProvider>();
            mockFileProvider.Setup(f => f.ReadAllTextAsync(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException("File not found"));
            
            var service = new ProjectDataService(mockFileProvider.Object);
            var exception = await Assert.ThrowsAsync<DataLoadException>(
                () => service.LoadProjectDataAsync("missing.json"));
            
            Assert.NotNull(exception.InnerException);
        }
    }
}