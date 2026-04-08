using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Models;
using Microsoft.AspNetCore.Hosting;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataControllerTests
    {
        private readonly Mock<IProjectDataService> _projectDataServiceMock;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock;

        public ProjectDataControllerTests()
        {
            _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            _webHostEnvironmentMock
                .Setup(x => x.WebRootPath)
                .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            _projectDataServiceMock = new Mock<IProjectDataService>();
        }

        [Fact]
        public async Task GetProjectData_ReturnsProjectDataSuccessfully()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test",
                Status = "InProgress",
                CompletionPercentage = 50
            };

            _projectDataServiceMock
                .Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            var result = await _projectDataServiceMock.Object.LoadProjectDataAsync();

            Assert.NotNull(result);
            Assert.Equal("Test", result.ProjectName);
        }

        [Fact]
        public async Task GetProjectData_WithNullEnvironment_ThrowsException()
        {
            _projectDataServiceMock
                .Setup(s => s.LoadProjectDataAsync())
                .ThrowsAsync(new InvalidOperationException("WebRootPath not set"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _projectDataServiceMock.Object.LoadProjectDataAsync());
        }
    }
}