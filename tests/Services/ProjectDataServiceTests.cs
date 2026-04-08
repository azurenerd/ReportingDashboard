using Xunit;
using AgentSquad.Services;
using AgentSquad.Data;
using System.Text.Json;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly ProjectDataService _service;
        private readonly string _testDataPath;

        public ProjectDataServiceTests()
        {
            _service = new ProjectDataService();
            _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
        }

        [Fact]
        public async Task LoadProjectDataAsync_LoadsValidJsonFile()
        {
            var filePath = Path.Combine(_testDataPath, "valid_project.json");
            File.WriteAllText(filePath, """{"name":"Test","status":"Active","completionPercentage":50}""");

            var result = await _service.LoadProjectDataAsync(filePath);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
            Assert.Equal(50, result.CompletionPercentage);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsFileNotFoundException()
        {
            var filePath = Path.Combine(_testDataPath, "nonexistent.json");
            var ex = await Assert.ThrowsAsync<FileNotFoundException>(() => _service.LoadProjectDataAsync(filePath));
            Assert.Contains("data.json not found", ex.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsOnInvalidJson()
        {
            var filePath = Path.Combine(_testDataPath, "invalid.json");
            File.WriteAllText(filePath, "{invalid json content}");

            var ex = await Assert.ThrowsAsync<JsonException>(() => _service.LoadProjectDataAsync(filePath));
            Assert.Contains("Invalid JSON format", ex.Message);
        }

        [Fact]
        public void GetCachedData_ReturnsNullBeforeLoad()
        {
            var result = _service.GetCachedData();
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCachedData_ReturnsPreviouslyLoadedData()
        {
            var filePath = Path.Combine(_testDataPath, "cached_project.json");
            File.WriteAllText(filePath, """{"name":"Cached","status":"Active","completionPercentage":75}""");

            await _service.LoadProjectDataAsync(filePath);
            var cached = _service.GetCachedData();
            Assert.NotNull(cached);
            Assert.Equal("Cached", cached.Name);
        }

        [Fact]
        public async Task RefreshData_ClearsCacheAndResetsTimestamp()
        {
            var filePath = Path.Combine(_testDataPath, "refresh_project.json");
            File.WriteAllText(filePath, """{"name":"Refresh","status":"Active","completionPercentage":25}""");

            await _service.LoadProjectDataAsync(filePath);
            _service.RefreshData();
            var cached = _service.GetCachedData();
            Assert.Null(cached);
        }

        [Fact]
        public async Task LoadProjectDataAsync_PersistsCacheAcrossMultipleCalls()
        {
            var filePath = Path.Combine(_testDataPath, "persistent_project.json");
            File.WriteAllText(filePath, """{"name":"Persistent","status":"Active","completionPercentage":60}""");

            await _service.LoadProjectDataAsync(filePath);
            var first = _service.GetCachedData();
            var second = _service.GetCachedData();
            
            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(first.Name, second.Name);
        }
    }
}