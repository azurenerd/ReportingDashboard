using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class DashboardLayoutTests : TestContext
    {
        private readonly Mock<IDataProvider> _mockDataProvider;

        public DashboardLayoutTests()
        {
            _mockDataProvider = new Mock<IDataProvider>();
        }

        private IRenderedComponent<DashboardLayout> RenderComponent()
        {
            Services.AddScoped(_ => _mockDataProvider.Object);
            return RenderComponent<DashboardLayout>();
        }

        [Fact]
        public async Task OnInitializedAsync_LoadsProjectDataSuccessfully()
        {
            var projectData = new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                LastUpdated = DateTime.Now,
                Milestones = new(),
                Metrics = new ProjectMetrics(),
                WorkItems = new()
            };

            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            var component = RenderComponent();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.NotNull(component.Instance.ProjectData);
            Assert.Equal("Test Project", component.Instance.ProjectData.Name);
            Assert.False(component.Instance.IsLoading);
            Assert.Null(component.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_DisplaysLoadingState()
        {
            var tcs = new TaskCompletionSource<Project>();
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .Returns(tcs.Task);

            var component = RenderComponent();
            
            var task = component.InvokeAsync(() => component.Instance.OnInitializedAsync());
            component.WaitForAssertion(() =>
            {
                Assert.True(component.Instance.IsLoading);
            });

            tcs.SetResult(new Project { Name = "Test" });
            await task;
        }

        [Fact]
        public async Task OnInitializedAsync_HandlesFileNotFoundException()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new FileNotFoundException("data.json not found"));

            var component = RenderComponent();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.False(component.Instance.IsLoading);
            Assert.NotNull(component.Instance.ErrorMessage);
            Assert.Contains("Configuration file not found", component.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_HandlesInvalidOperationException()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new InvalidOperationException("Invalid JSON format"));

            var component = RenderComponent();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.False(component.Instance.IsLoading);
            Assert.NotNull(component.Instance.ErrorMessage);
            Assert.Contains("Failed to parse project data", component.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_HandlesGenericException()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new Exception("Unexpected error"));

            var component = RenderComponent();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.False(component.Instance.IsLoading);
            Assert.NotNull(component.Instance.ErrorMessage);
            Assert.Contains("An unexpected error occurred", component.Instance.ErrorMessage);
        }

        [Fact]
        public async Task OnInitializedAsync_HandlesNullProjectData()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync((Project)null);

            var component = RenderComponent();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.False(component.Instance.IsLoading);
            Assert.NotNull(component.Instance.ErrorMessage);
            Assert.Equal("Project data is empty or invalid.", component.Instance.ErrorMessage);
        }

        [Fact]
        public void Renders_LoadingSpinner_WhenIsLoadingTrue()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(new Project());

            var component = RenderComponent();
            component.Instance.IsLoading = true;
            component.Render();

            Assert.Contains("Loading dashboard data", component.Markup);
            Assert.Contains("loading-state", component.Markup);
        }

        [Fact]
        public void Renders_ErrorMessage_WhenErrorOccurs()
        {
            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(new Project());

            var component = RenderComponent();
            component.Instance.ErrorMessage = "Test error message";
            component.Render();

            Assert.Contains("Error Loading Dashboard", component.Markup);
            Assert.Contains("Test error message", component.Markup);
        }

        [Fact]
        public void Renders_DashboardContent_WhenDataLoaded()
        {
            var projectData = new Project
            {
                Name = "Executive Dashboard",
                Description = "Project Overview",
                LastUpdated = DateTime.Now,
                Milestones = new(),
                Metrics = new ProjectMetrics(),
                WorkItems = new()
            };

            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            var component = RenderComponent();
            component.Instance.ProjectData = projectData;
            component.Instance.IsLoading = false;
            component.Render();

            Assert.Contains("Executive Dashboard", component.Markup);
            Assert.Contains("Project Overview", component.Markup);
            Assert.Contains("dashboard-grid", component.Markup);
            Assert.Contains("Timeline", component.Markup);
            Assert.Contains("Metrics", component.Markup);
            Assert.Contains("Work Items", component.Markup);
        }

        [Fact]
        public void Renders_AllChildComponents_WithCorrectData()
        {
            var projectData = new Project
            {
                Name = "Test Project",
                Description = "Description",
                LastUpdated = DateTime.Now,
                Milestones = new List<Milestone>
                {
                    new() { Name = "M1", TargetDate = DateTime.Now, Status = MilestoneStatus.InProgress }
                },
                Metrics = new ProjectMetrics { CompletionPercentage = 50 },
                WorkItems = new List<WorkItem>
                {
                    new() { Id = "1", Title = "Task 1", Status = WorkItemStatus.InProgress }
                }
            };

            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            var component = RenderComponent();
            component.Instance.ProjectData = projectData;
            component.Instance.IsLoading = false;
            component.Render();

            Assert.NotNull(component.Instance.ProjectData.Milestones);
            Assert.NotNull(component.Instance.ProjectData.Metrics);
            Assert.NotNull(component.Instance.ProjectData.WorkItems);
        }

        [Fact]
        public async Task AcceptanceCriteria_LoadsDataOnInitialization()
        {
            var projectData = new Project
            {
                Name = "Dashboard Project",
                Milestones = new(),
                Metrics = new(),
                WorkItems = new()
            };

            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            var component = RenderComponent();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            _mockDataProvider.Verify(x => x.LoadProjectDataAsync(), Times.Once);
            Assert.NotNull(component.Instance.ProjectData);
        }

        [Fact]
        public void AcceptanceCriteria_RendersCSSGridLayout()
        {
            var projectData = new Project
            {
                Name = "Grid Test",
                Milestones = new(),
                Metrics = new(),
                WorkItems = new()
            };

            _mockDataProvider.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            var component = RenderComponent();
            component.Instance.ProjectData = projectData;
            component.Instance.IsLoading = false;
            component.Render();

            Assert.Contains("dashboard-grid", component.Markup);
            Assert.Contains("dashboard-header", component.Markup);
            Assert.Contains("timeline-section", component.Markup);
            Assert.Contains("metrics-section", component.Markup);
            Assert.Contains("workitems-section", component.Markup);
        }
    }
}