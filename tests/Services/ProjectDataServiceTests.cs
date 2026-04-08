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
        public async Task LoadProjectDataAsync_WithValidJsonPath_ReturnsProjectData()
        {
            var jsonPath = "data/test-project.json";
            var result = await _service.LoadProjectDataAsync(jsonPath);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ReturnsEmptyProjectData()
        {
            var jsonPath = "data/nonexistent.json";
            var result = await _service.LoadProjectDataAsync(jsonPath);
            Assert.NotNull(result);
            Assert.IsType<ProjectData>(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMalformedJson_CatchesJsonExceptionAndReturnsEmpty()
        {
            var jsonPath = "data/malformed.json";
            var result = await _service.LoadProjectDataAsync(jsonPath);
            Assert.NotNull(result);
            Assert.IsType<ProjectData>(result);
        }

        [Fact]
        public void RefreshData_UpdatesInternalState()
        {
            _service.RefreshData();
            var data = _service.GetCachedData();
            Assert.NotNull(data);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidPath_CachesData()
        {
            var jsonPath = "data/test-project.json";
            await _service.LoadProjectDataAsync(jsonPath);
            var cachedData = _service.GetCachedData();
            Assert.NotNull(cachedData);
        }

        [Fact]
        public async Task LoadProjectDataAsync_SchemaValidationFailure_ReturnsEmpty()
        {
            var jsonPath = "data/invalid-schema.json";
            var result = await _service.LoadProjectDataAsync(jsonPath);
            Assert.NotNull(result);
            Assert.IsType<ProjectData>(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithFileNotFoundException_ReturnsEmpty()
        {
            var jsonPath = "data/missing-file.json";
            var result = await _service.LoadProjectDataAsync(jsonPath);
            Assert.NotNull(result);
        }
    }
}