using Xunit;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class FullApplicationIntegrationTests : IAsyncLifetime
    {
        private readonly string _tempDataDir;
        private WebApplicationFactory<Program>? _factory;
        private HttpClient? _client;

        public FullApplicationIntegrationTests()
        {
            _tempDataDir = Path.Combine(Path.GetTempPath(), $"app_test_{Guid.NewGuid()}");
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_tempDataDir);
            CreateValidDataFile();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureServices(services =>
                    {
                    });
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        context.HostingEnvironment.ContentRootPath = _tempDataDir;
                    });
                });

            _client = _factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
            if (Directory.Exists(_tempDataDir))
            {
                Directory.Delete(_tempDataDir, recursive: true);
            }
        }

        private void CreateValidDataFile()
        {
            var dataDir = Path.Combine(_tempDataDir, "data");
            Directory.CreateDirectory(dataDir);

            var json = """
            {
              "projectName": "Integration Test Project",
              "description": "Test project for integration testing",
              "startDate": "2026-01-15",
              "plannedCompletion": "2026-06-30",
              "milestones": [
                {"id":"m1","name":"Phase 1","targetDate":"2026-02-28","status":"Completed"},
                {"id":"m2","name":"Phase 2","targetDate":"2026-04-15","status":"OnTrack"}
              ],
              "shipped": [
                {"id":"w1","title":"Feature 1","completedDate":"2026-02-10"},
                {"id":"w2","title":"Feature 2","completedDate":"2026-02-20"}
              ],
              "inProgress": [
                {"id":"w3","title":"Feature 3"}
              ],
              "carriedOver": [
                {"id":"w4","title":"Feature 4"}
              ]
            }
            """;

            File.WriteAllText(Path.Combine(dataDir, "data.json"), json);
        }

        [Fact]
        public async Task Application_StartsSuccessfully_WithValidDataFile()
        {
            var response = await _client!.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Application_LoadsDashboard_WithCorrectData()
        {
            var response = await _client!.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("Integration Test Project", content);
            Assert.Contains("Phase 1", content);
            Assert.Contains("Feature 1", content);
        }

        [Fact]
        public async Task ServiceDependencies_WireCorrectly_InDIContainer()
        {
            using var scope = _factory!.Services.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IProjectDataService>();

            Assert.True(dataService.IsInitialized);
            Assert.Null(dataService.LastError);
        }

        [Fact]
        public async Task ProjectDataService_LoadsDashboard_FromJsonFile()
        {
            using var scope = _factory!.Services.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IProjectDataService>();

            var dashboard = dataService.GetDashboard();

            Assert.Equal("Integration Test Project", dashboard.ProjectName);
            Assert.Equal(2, dashboard.Milestones.Count);
            Assert.Equal(2, dashboard.Shipped.Count);
            Assert.Single(dashboard.InProgress);
            Assert.Single(dashboard.CarriedOver);
        }

        [Fact]
        public async Task ProjectDataService_CalculatesMetrics_Correctly()
        {
            using var scope = _factory!.Services.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IProjectDataService>();

            var dashboard = dataService.GetDashboard();

            Assert.Equal(5, dashboard.Metrics.TotalPlanned);
            Assert.Equal(2, dashboard.Metrics.Completed);
            Assert.Equal(3, dashboard.Metrics.InFlight);
            Assert.Equal(40m, dashboard.Metrics.HealthScore);
        }

        [Fact]
        public async Task Application_Handles_Missing_DataFile_Gracefully()
        {
            var errorDataDir = Path.Combine(_tempDataDir, "error_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(errorDataDir);

            var errorFactory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        context.HostingEnvironment.ContentRootPath = errorDataDir;
                    });
                });

            using var scope = errorFactory.Services.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IProjectDataService>();

            Assert.False(dataService.IsInitialized);
            Assert.NotNull(dataService.LastError);
            Assert.Contains("not found", dataService.LastError);

            errorFactory.Dispose();
        }
    }
}