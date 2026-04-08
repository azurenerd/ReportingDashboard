using Bunit;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration
{
    public class DashboardIntegrationTests : TestContext
    {
        [Fact]
        public async Task FullDashboard_LoadsAndRendersAllSections()
        {
            var jsonContent = @"{
                ""name"": ""Integration Test Project"",
                ""description"": ""Full Dashboard Test"",
                ""milestones"": [
                    {
                        ""name"": ""Alpha"",
                        ""targetDate"": ""2026-06-01T00:00:00"",
                        ""status"": 1,
                        ""description"": ""Alpha release""
                    },
                    {
                        ""name"": ""Beta"",
                        ""targetDate"": ""2026-08-01T00:00:00"",
                        ""status"": 3,
                        ""description"": ""Beta release""
                    }
                ],
                ""metrics"": {
                    ""completionPercentage"": 50,
                    ""healthStatus"": 0,
                    ""velocityThisMonth"": 8,
                    ""completedMilestones"": 1,
                    ""milestoneCount"": 2,
                    ""targetMilestoneCount"": 4
                },
                ""workItems"": [
                    {
                        ""id"": ""1"",
                        ""title"": ""Feature A"",
                        ""description"": ""Feature A description"",
                        ""status"": 0
                    },
                    {
                        ""id"": ""2"",
                        ""title"": ""Feature B"",
                        ""description"": ""Feature B description"",
                        ""status"": 1
                    },
                    {
                        ""id"": ""3"",
                        ""title"": ""Feature C"",
                        ""description"": ""Feature C description"",
                        ""status"": 2
                    }
                ]
            }";

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var dataFilePath = Path.Combine(tempDir, "data.json");
            File.WriteAllText(dataFilePath, jsonContent);

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(x => x.WebRootPath).Returns(tempDir);

            Services.AddMemoryCache();
            Services.AddScoped<IDataCache, MemoryCacheProvider>();
            Services.AddScoped<IDataProvider>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DataProvider>>();
                return new DataProvider(logger, mockEnv.Object, sp.GetRequiredService<IDataCache>());
            });

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.NotNull(component.Instance.ProjectData);
            Assert.Equal("Integration Test Project", component.Instance.ProjectData.Name);
            Assert.Equal(2, component.Instance.ProjectData.Milestones.Count);
            Assert.Equal(3, component.Instance.ProjectData.WorkItems.Count);

            var markup = component.Markup;
            Assert.Contains("Integration Test Project", markup);
            Assert.Contains("Alpha", markup);
            Assert.Contains("Beta", markup);
            Assert.Contains("Feature A", markup);
            Assert.Contains("Feature B", markup);
            Assert.Contains("Feature C", markup);

            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task Dashboard_HandlesErrorGracefully_OnLoadFailure()
        {
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(x => x.WebRootPath).Returns("/nonexistent");

            var mockLogger = new Mock<ILogger<DataProvider>>();
            var mockCache = new Mock<IDataCache>();

            Services.AddScoped<IDataProvider>(sp =>
                new DataProvider(mockLogger.Object, mockEnv.Object, mockCache.Object)
            );

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.False(component.Instance.IsLoading);
            Assert.NotNull(component.Instance.ErrorMessage);
            Assert.Contains("error-state", component.Markup);
        }
    }
}