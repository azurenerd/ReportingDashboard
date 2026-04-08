using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using AgentSquad.Services;

namespace AgentSquad.Tests.Integration
{
    /// <summary>
    /// Integration tests for WorkItemSummary component with DashboardLayout and services.
    /// Validates data flow from DataProvider through component rendering.
    /// </summary>
    public class WorkItemSummaryIntegrationTests : TestContext
    {
        [Fact]
        public async Task WorkItemSummaryReceivesDataFromDashboardLayout()
        {
            // Arrange
            var mockData = new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                Milestones = new List<Milestone>
                {
                    new() { Name = "M1", Status = MilestoneStatus.Completed, TargetDate = System.DateTime.Now }
                },
                WorkItems = new List<WorkItem>
                {
                    new() { Title = "Shipped Item", Status = WorkItemStatus.Shipped, Description = "Done" },
                    new() { Title = "In Progress Item", Status = WorkItemStatus.InProgress, Description = "Working" },
                    new() { Title = "Carried Over Item", Status = WorkItemStatus.CarriedOver, Description = "Delayed" }
                }
            };

            var mockDataProvider = new MockDataProvider(mockData);
            Services.AddScoped<IDataProvider>(_ => mockDataProvider);
            Services.AddScoped<ILogger<DashboardLayout>>(sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<DashboardLayout>());
            Services.AddScoped<ILogger<DataProvider>>(sp =>
                sp.GetRequiredService<ILoggerFactory>().CreateLogger<DataProvider>());

            // Act
            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            // Assert - WorkItemSummary should render with all three columns
            var columns = cut.FindAll(".work-item-column");
            Assert.Equal(3, columns.Count);

            var items = cut.FindAll(".work-item");
            Assert.Equal(3, items.Count);
        }

        /// <summary>
        /// Mock DataProvider for testing without file I/O.
        /// </summary>
        private class MockDataProvider : IDataProvider
        {
            private readonly Project _projectData;

            public MockDataProvider(Project projectData)
            {
                _projectData = projectData;
            }

            public Task<Project> LoadProjectDataAsync()
            {
                return Task.FromResult(_projectData);
            }

            public void InvalidateCache()
            {
                // No-op for tests
            }
        }
    }
}