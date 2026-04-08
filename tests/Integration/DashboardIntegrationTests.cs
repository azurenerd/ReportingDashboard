using Xunit;
using AgentSquad.Services;
using AgentSquad.Data;

namespace AgentSquad.Tests.Integration
{
    public class DashboardIntegrationTests
    {
        private readonly ProjectDataService _service;
        private readonly string _testDataPath;

        public DashboardIntegrationTests()
        {
            _service = new ProjectDataService();
            _testDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
            Directory.CreateDirectory(_testDataPath);
        }

        [Fact]
        public async Task Dashboard_Integration_LoadsAndDeserializesProjectData()
        {
            var filePath = Path.Combine(_testDataPath, "dashboard_integration.json");
            var jsonContent = """
            {
                "name": "Integration Test Project",
                "status": "Active",
                "completionPercentage": 45,
                "milestones": [
                    {"name": "Phase 1", "startDate": "2026-03-01", "targetDate": "2026-04-01"}
                ]
            }
            """;
            File.WriteAllText(filePath, jsonContent);

            var result = await _service.LoadProjectDataAsync(filePath);
            
            Assert.NotNull(result);
            Assert.Equal("Integration Test Project", result.Name);
            Assert.Equal("Active", result.Status);
            Assert.Equal(45, result.CompletionPercentage);
            Assert.NotNull(result.Milestones);
            Assert.Single(result.Milestones);
        }

        [Fact]
        public async Task Dashboard_Integration_HandlesFileNotFound()
        {
            var filePath = Path.Combine(_testDataPath, "does_not_exist.json");
            
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _service.LoadProjectDataAsync(filePath));
        }

        [Fact]
        public async Task Dashboard_Integration_HandlesInvalidJson()
        {
            var filePath = Path.Combine(_testDataPath, "malformed.json");
            File.WriteAllText(filePath, "{ invalid: json }");

            await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => 
                _service.LoadProjectDataAsync(filePath));
        }

        [Fact]
        public async Task Dashboard_Integration_CachePersistedAcrossServiceCalls()
        {
            var filePath = Path.Combine(_testDataPath, "cache_test.json");
            File.WriteAllText(filePath, """{"name":"CacheTest","status":"Active","completionPercentage":30}""");

            var loaded = await _service.LoadProjectDataAsync(filePath);
            var cached1 = _service.GetCachedData();
            var cached2 = _service.GetCachedData();

            Assert.Equal(loaded.Name, cached1.Name);
            Assert.Equal(cached1.Name, cached2.Name);
        }

        [Fact]
        public async Task Dashboard_Integration_RefreshReloadsFromFile()
        {
            var filePath = Path.Combine(_testDataPath, "refresh_test.json");
            File.WriteAllText(filePath, """{"name":"Original","status":"Active","completionPercentage":20}""");

            await _service.LoadProjectDataAsync(filePath);
            var original = _service.GetCachedData();

            File.WriteAllText(filePath, """{"name":"Updated","status":"Paused","completionPercentage":50}""");
            _service.RefreshData();
            var refreshed = await _service.LoadProjectDataAsync(filePath);

            Assert.Equal("Original", original.Name);
            Assert.Equal("Updated", refreshed.Name);
        }
    }
}