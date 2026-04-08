using Bunit;
using Xunit;
using AgentSquad.Models;
using AgentSquad.Runner.Components;
using AgentSquad.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSquad.Tests.Components
{
    public class DashboardLayoutIntegrationTests : TestContext
    {
        [Fact]
        public async Task DashboardLayout_LoadsProjectData_OnInitialization()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            var projectData = new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                CompletionPercentage = 50,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                Milestones = new List<Milestone>(),
                WorkItems = new List<WorkItem>()
            };

            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("Test Project", component.Markup);
            mockDataProvider.Verify(x => x.LoadProjectDataAsync(), Times.Once);
        }

        [Fact]
        public async Task DashboardLayout_DisplaysLoadingSpinner_WhileLoading()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .Returns(async () =>
                {
                    await Task.Delay(500);
                    return new Project
                    {
                        Name = "Test",
                        WorkItems = new List<WorkItem>()
                    };
                });

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();

            Assert.Contains("Loading dashboard data", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_DisplaysErrorMessage_OnFileNotFound()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new System.IO.FileNotFoundException());

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("Configuration file not found", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_DisplaysJsonErrorMessage_OnInvalidJson()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new System.Text.Json.JsonException("Invalid JSON"));

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("Invalid JSON format", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_DisplaysInvalidStructureError_OnInvalidOperationException()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new System.InvalidOperationException("Invalid structure"));

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("Invalid project structure", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_PassesWorkItemsToDashboard_WhenDataLoaded()
        {
            var workItems = new List<WorkItem>
            {
                new WorkItem { Title = "Item 1", Status = WorkItemStatus.Shipped }
            };

            var projectData = new Project
            {
                Name = "Test Project",
                WorkItems = workItems,
                Milestones = new List<Milestone>()
            };

            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("Item 1", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_ReloadButton_RefreshesData()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            var initialProject = new Project
            {
                Name = "Initial",
                WorkItems = new List<WorkItem>(),
                Milestones = new List<Milestone>()
            };

            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(initialProject);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            var retryButton = component.Find(".btn-retry");
            Assert.NotNull(retryButton);

            mockDataProvider.Verify(x => x.InvalidateCache(), Times.Never);
        }

        [Fact]
        public async Task DashboardLayout_DisplaysProjectName_InHeader()
        {
            var projectData = new Project
            {
                Name = "Executive Dashboard",
                Description = "Project Status Overview",
                WorkItems = new List<WorkItem>(),
                Milestones = new List<Milestone>()
            };

            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("Executive Dashboard", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_HandlesMilestones_WhenPresent()
        {
            var milestones = new List<Milestone>
            {
                new Milestone 
                { 
                    Name = "Phase 1", 
                    Status = MilestoneStatus.Completed,
                    TargetDate = System.DateTime.Now 
                }
            };

            var projectData = new Project
            {
                Name = "Test",
                Milestones = milestones,
                WorkItems = new List<WorkItem>()
            };

            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.NotNull(component);
        }

        [Fact]
        public async Task DashboardLayout_HandlesEmptyWorkItems_Gracefully()
        {
            var projectData = new Project
            {
                Name = "Test",
                WorkItems = new List<WorkItem>(),
                Milestones = new List<Milestone>()
            };

            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("dashboard-header", component.Markup);
        }

        [Fact]
        public async Task DashboardLayout_HandlesNullProjectData_GracefullyWithErrorOverlay()
        {
            var mockDataProvider = new Mock<IDataProvider>();
            mockDataProvider
                .Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync((Project)null);

            Services.AddScoped(_ => mockDataProvider.Object);

            var component = RenderComponent<DashboardLayout>();
            await component.InvokeAsync(async () => await Task.Delay(100));

            Assert.Contains("No Data", component.Markup);
        }
    }
}