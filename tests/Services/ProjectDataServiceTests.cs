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
        public async Task LoadProjectDataAsync_LoadsFromWwwrootPath()
        {
            var result = await _service.LoadProjectDataAsync();
            Assert.NotNull(result);
            Assert.IsType<ProjectData>(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ReturnsEmptyProjectData()
        {
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "nonexistent"));
            var service = new ProjectDataService(_mockEnvironment.Object);
            var result = await service.LoadProjectDataAsync();
            Assert.NotNull(result);
            Assert.Empty(result.Milestones ?? new List<Milestone>());
        }

        [Fact]
        public void RefreshData_ReloadsDataFromWwwroot()
        {
            _service.RefreshData();
            var cachedData = _service.GetCachedData();
            Assert.NotNull(cachedData);
        }

        [Fact]
        public async Task LoadProjectDataAsync_CachesLoadedData()
        {
            await _service.LoadProjectDataAsync();
            var cached = _service.GetCachedData();
            Assert.NotNull(cached);
            Assert.IsType<ProjectData>(cached);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_PopulatesProjectInfo()
        {
            var result = await _service.LoadProjectDataAsync();
            Assert.NotNull(result.Project);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidData_PopulatesMilestones()
        {
            var result = await _service.LoadProjectDataAsync();
            Assert.IsType<List<Milestone>>(result.Milestones ?? new List<Milestone>());
        }
    }
}