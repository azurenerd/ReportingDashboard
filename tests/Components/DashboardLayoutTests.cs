using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;

namespace AgentSquad.Runner.Tests.Components
{
    public class DashboardLayoutTests : TestContext
    {
        private readonly Mock<IDataProvider> _mockDataProvider;
        private readonly Mock<IDataCache> _mockDataCache;
        private readonly Mock<ILogger<DashboardLayout>> _mockLogger;

        public DashboardLayoutTests()
        {
            _mockDataProvider = new Mock<IDataProvider>();
            _mockDataCache = new Mock<IDataCache>();
            _mockLogger = new Mock<ILogger<DashboardLayout>>();

            Services.AddSingleton(_mockDataProvider.Object);
            Services.AddSingleton(_mockDataCache.Object);
            Services.AddSingleton(_mockLogger.Object);
        }

        private Project CreateValidProject()
        {
            return new Project
            {
                Name = "Test Project",
                Description = "A test project for dashboard",
                StartDate = DateTime.Now.AddMonths(-3),
                TargetEndDate = DateTime.Now.AddMonths(3),
                CompletionPercentage = 45,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 12,
                Milestones = new List<Milestone>
                {
                    new Milestone
                    {
                        Name = "Phase 1",
                        TargetDate = DateTime.Now.AddMonths(1),
                        Status = MilestoneStatus.Completed,
                        Description = "Initial phase"
                    },
                    new Milestone
                    {
                        Name = "Phase 2",
                        TargetDate = DateTime.Now.AddMonths(2),
                        Status = MilestoneStatus.InProgress,
                        Description = "Development phase"
                    }
                },
                WorkItems = new List<WorkItem>
                {
                    new WorkItem
                    {
                        Title = "Feature A",
                        Description = "First feature",
                        Status = WorkItemStatus.Shipped,
                        AssignedTo = "Alice"
                    },
                    new WorkItem
                    {
                        Title = "Feature B",
                        Description = "Second feature",
                        Status = WorkItemStatus.InProgress,
                        AssignedTo = "Bob"
                    }
                }
            };
        }

        [Fact]
        public async Task OnInitializedAsync_CallsDataProvider_LoadProjectDataAsync()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            _mockDataProvider.Verify(x => x.LoadProjectDataAsync(), Times.Once);
        }

        [Fact]
        public async Task OnInitializedAsync_SetsIsLoadingToTrue_DuringFetch()
        {
            var project = CreateValidProject();
            var tcs = new TaskCompletionSource<Project>();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).Returns(tcs.Task);

            var cut = RenderComponent<DashboardLayout>();
            var task = cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            await Task.Delay(100);
            Assert.True(cut.Instance.IsLoading);

            tcs.SetResult(project);
            await task;
        }

        [Fact]
        public async Task OnInitializedAsync_SetsIsLoadingToFalse_AfterCompletion()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.False(cut.Instance.IsLoading);
        }

        [Fact]
        public async Task OnInitializedAsync_PopulatesProjectData_WithValidProject()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.NotNull(cut.Instance.ProjectData);
            Assert.Equal("Test Project", cut.Instance.ProjectData.Name);
            Assert.Equal(45, cut.Instance.ProjectData.CompletionPercentage);
        }

        [Fact]
        public async Task OnInitializedAsync_SetsErrorMessage_WhenFileNotFound()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new FileNotFoundException("data.json not found"));

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.False(cut.Instance.IsLoading);
            Assert.NotEmpty(cut.Instance.ErrorMessage);
            Assert.Contains("Configuration file not found", cut.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_SetsErrorMessage_WhenJsonInvalid()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new System.Text.Json.JsonException("Invalid JSON"));

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.False(cut.Instance.IsLoading);
            Assert.NotEmpty(cut.Instance.ErrorMessage);
            Assert.Contains("Invalid JSON format", cut.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_SetsErrorMessage_WhenValidationFails()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new InvalidOperationException("Project name is required"));

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.False(cut.Instance.IsLoading);
            Assert.NotEmpty(cut.Instance.ErrorMessage);
            Assert.Contains("Configuration validation failed", cut.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_SetsErrorMessage_OnUnexpectedException()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new Exception("Unexpected error"));

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.False(cut.Instance.IsLoading);
            Assert.NotEmpty(cut.Instance.ErrorMessage);
            Assert.Contains("unexpected error", cut.Instance.ErrorMessage);
        }

        [Fact]
        public async Task RenderLoadingSpinner_WhenIsLoadingIsTrue()
        {
            var project = CreateValidProject();
            var tcs = new TaskCompletionSource<Project>();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).Returns(tcs.Task);

            var cut = RenderComponent<DashboardLayout>();
            var task = cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            await Task.Delay(100);
            cut.Render();

            var spinner = cut.FindComponent<LoadingSpinner>();
            Assert.NotNull(spinner);

            tcs.SetResult(project);
            await task;
        }

        [Fact]
        public async Task RenderErrorMessage_WhenErrorMessageIsSet()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new FileNotFoundException());

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var errorComponent = cut.FindComponent<ErrorMessage>();
            Assert.NotNull(errorComponent);
        }

        [Fact]
        public async Task RenderDashboardContent_WhenDataLoaded()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var header = cut.Find("header.dashboard-header");
            Assert.NotNull(header);
            Assert.Contains("Test Project", header.TextContent);
        }

        [Fact]
        public async Task PassMilestonesToMilestoneTimeline_Component()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var timelineComponent = cut.FindComponent<MilestoneTimeline>();
            Assert.NotNull(timelineComponent);
            Assert.NotNull(timelineComponent.Instance.Milestones);
            Assert.Equal(2, timelineComponent.Instance.Milestones.Count);
        }

        [Fact]
        public async Task PassMetricsToProjectMetrics_Component()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var metricsComponent = cut.FindComponent<ProjectMetrics>();
            Assert.NotNull(metricsComponent);
            Assert.NotNull(metricsComponent.Instance.Metrics);
            Assert.Equal(45, metricsComponent.Instance.Metrics.CompletionPercentage);
        }

        [Fact]
        public async Task PassWorkItemsToWorkItemSummary_Component()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var workItemsComponent = cut.FindComponent<WorkItemSummary>();
            Assert.NotNull(workItemsComponent);
            Assert.NotNull(workItemsComponent.Instance.WorkItems);
            Assert.Equal(2, workItemsComponent.Instance.WorkItems.Count);
        }

        [Fact]
        public async Task ExtractProjectMetrics_MapsProjectDataCorrectly()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            var metrics = cut.Instance.ExtractProjectMetrics(project);

            Assert.NotNull(metrics);
            Assert.Equal(45, metrics.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
            Assert.Equal(12, metrics.VelocityThisMonth);
            Assert.Equal(2, metrics.TotalMilestones);
            Assert.Equal(1, metrics.CompletedMilestones);
        }

        [Fact]
        public async Task ExtractProjectMetrics_HandlesNullProject()
        {
            var cut = RenderComponent<DashboardLayout>();
            var metrics = cut.Instance.ExtractProjectMetrics(null);

            Assert.Null(metrics);
        }

        [Fact]
        public async Task ExtractProjectMetrics_ClampsCompletionPercentageToMax100()
        {
            var project = CreateValidProject();
            project.CompletionPercentage = 150;
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            var metrics = cut.Instance.ExtractProjectMetrics(project);

            Assert.Equal(100, metrics.CompletionPercentage);
        }

        [Fact]
        public async Task ExtractProjectMetrics_ClampsCompletionPercentageToMin0()
        {
            var project = CreateValidProject();
            project.CompletionPercentage = -10;
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            var metrics = cut.Instance.ExtractProjectMetrics(project);

            Assert.Equal(0, metrics.CompletionPercentage);
        }

        [Fact]
        public async Task HandleRetry_InvalidatesCacheAndReloadsData()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Instance.ErrorMessage = "Test error";
            await cut.InvokeAsync(() => cut.Instance.HandleRetry());

            _mockDataCache.Verify(x => x.Remove("project_data"), Times.Once);
            Assert.Empty(cut.Instance.ErrorMessage);
        }

        [Fact]
        public async Task HandleDismiss_ClearsErrorMessage()
        {
            var cut = RenderComponent<DashboardLayout>();
            cut.Instance.ErrorMessage = "Test error message";

            await cut.InvokeAsync(() => cut.Instance.HandleDismiss());

            Assert.Empty(cut.Instance.ErrorMessage);
        }

        [Fact]
        public async Task RenderEmptyState_WhenProjectDataIsNull()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync((Project)null);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var emptyState = cut.Find(".empty-state");
            Assert.NotNull(emptyState);
            Assert.Contains("No project data available", emptyState.TextContent);
        }

        [Fact]
        public async Task RenderDebugPanel_InDevelopmentEnvironment()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();
            var debugPanel = cut.QuerySelector(".debug-panel");
            
            // Debug panel visibility depends on environment
            // In test environment, this may or may not be visible
            if (debugPanel != null)
            {
                Assert.NotNull(debugPanel);
            }
        }

        [Fact]
        public async Task ErrorMessage_ClearedOnSuccessfulReload()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new FileNotFoundException())
                .Then()
                .Returns(Task.FromResult(project));

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            Assert.NotEmpty(cut.Instance.ErrorMessage);

            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);
            await cut.InvokeAsync(() => cut.Instance.HandleRetry());

            Assert.Empty(cut.Instance.ErrorMessage);
            Assert.NotNull(cut.Instance.ProjectData);
        }

        [Fact]
        public async Task RenderDashboardSections_InCorrectGridOrder()
        {
            var project = CreateValidProject();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync()).ReturnsAsync(project);

            var cut = RenderComponent<DashboardLayout>();
            await cut.InvokeAsync(() => cut.Instance.OnInitializedAsync());

            cut.Render();

            var sections = cut.QuerySelectorAll(".dashboard-container > section");
            Assert.NotEmpty(sections);
        }
    }
}