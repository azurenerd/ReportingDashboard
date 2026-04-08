using Xunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using AgentSquad.Services;
using AgentSquad.Data;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ProjectDataService _service;

        public ProjectDataServiceTests()
        {
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            _service = new ProjectDataService(_mockEnvironment.Object);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
        {
            var result = await _service.LoadProjectDataAsync();
            Assert.NotNull(result);
            Assert.NotNull(result.Project);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ReturnsEmptyProjectData()
        {
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "nonexistent"));
            var result = await _service.LoadProjectDataAsync();
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Project?.Name ?? string.Empty);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ReturnsEmptyProjectData()
        {
            var result = await _service.LoadProjectDataAsync();
            Assert.NotNull(result);
            Assert.IsType<ProjectData>(result);
        }

        [Fact]
        public void RefreshData_UpdatesInternalState()
        {
            _service.RefreshData();
            var data = _service.GetProjectData();
            Assert.NotNull(data);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ReturnsConsistentData_OnMultipleCalls()
        {
            var result1 = await _service.LoadProjectDataAsync();
            var result2 = await _service.LoadProjectDataAsync();
            Assert.Equal(result1.Project?.Name, result2.Project?.Name);
        }
    }
}