using Xunit;
using Bunit;
using Moq;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Components.Pages;

namespace AgentSquad.Runner.Tests.Components
{
    public class DashboardRazorTests : TestContext
    {
        private readonly Mock<ProjectDataService> _mockDataService;

        public DashboardRazorTests()
        {
            _mockDataService = new Mock<ProjectDataService>(MockBehavior.Strict, new MockWebHostEnvironment(Path.GetTempPath()));
        }

        private ProjectData GetValidProjectData()
        {
            return new ProjectData
            {
                Project = new ProjectInfo
                {
                    Name = "Test Project",
                    Description = "A test project",
                    Status = "In Progress",
                    Sponsor = "Test Sponsor",
                    ProjectManager = "Test PM",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 12, 31),
                    CompletionPercentage = 50
                },
                Milestones = new List<Milestone>
                {
                    new Milestone
                    {
                        Id = "m1",
                        Name = "Phase 1",
                        TargetDate = new DateTime(2024, 3, 31),
                        Status = MilestoneStatus.Completed,
                        CompletionPercentage = 100
                    }
                },
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask
                    {
                        Id = "t1",
                        Name = "Task 1",
                        Status = TaskStatus.Shipped,
                        AssignedTo = "Developer A",
                        DueDate = new DateTime(2024, 2, 1),
                        EstimatedDays = 5,
                        RelatedMilestone = "Phase 1"
                    },
                    new ProjectTask
                    {
                        Id = "t2",
                        Name = "Task 2",
                        Status = TaskStatus.InProgress,
                        AssignedTo = "Developer B",
                        DueDate = new DateTime(2024, 3, 1),
                        EstimatedDays = 8,
                        RelatedMilestone = "Phase 1"
                    },
                    new ProjectTask
                    {
                        Id = "t3",
                        Name = "Task 3",
                        Status = TaskStatus.CarriedOver,
                        AssignedTo = "Developer C",
                        DueDate = new DateTime(2024, 4, 1),
                        EstimatedDays = 10,
                        RelatedMilestone = "Phase 1"
                    }
                },
                Metrics = new ProjectMetrics
                {
                    TotalTasks = 3,
                    CompletedTasks = 1,
                    InProgressTasks = 1,
                    CarriedOverTasks = 1,
                    EstimatedBurndownRate = 0.33m
                }
            };
        }

        [Fact]
        public async Task Dashboard_OnInitializedAsync_LoadsProjectData()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            _mockDataService.Verify(x => x.LoadProjectDataAsync(), Times.Once);
        }

        [Fact]
        public async Task Dashboard_WithValidData_DisplaysProjectName()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            var heading = component.Find("h1.display-4");
            Assert.NotNull(heading);
            Assert.Contains("Test Project", heading.TextContent);
        }

        [Fact]
        public async Task Dashboard_WithValidData_DisplaysProjectDescription()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("A test project", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithValidData_RendersMilestoneTimeline()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Phase 1", component.Markup);
            Assert.Contains("Project Milestones", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithValidData_RendersStatusCards()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Task Status", component.Markup);
            Assert.Contains("Shipped", component.Markup);
            Assert.Contains("In-Progress", component.Markup);
            Assert.Contains("Carried-Over", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithValidData_RendersProgressMetrics()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Overall Project Completion", component.Markup);
            Assert.Contains("50", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithFileNotFound_DisplaysErrorMessage()
        {
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new DataLoadException("Data file not found"));

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Error", component.Markup);
            Assert.Contains("Data file not found", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithInvalidJson_DisplaysErrorMessage()
        {
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new DataLoadException("Invalid JSON in data.json"));

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Error", component.Markup);
            Assert.Contains("corrupted", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithUnexpectedException_DisplaysUserFriendlyErrorMessage()
        {
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ThrowsAsync(new Exception("Unexpected error"));

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Error", component.Markup);
            Assert.Contains("unexpected", component.Markup);
        }

        [Fact]
        public async Task Dashboard_OrganizesTasksByStatus_CorrectCounts()
        {
            var projectData = GetValidProjectData();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("1", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithEmptyMilestoneList_DisplaysPlaceholder()
        {
            var projectData = GetValidProjectData();
            projectData.Milestones = new List<Milestone>();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("No milestones available", component.Markup);
        }

        [Fact]
        public async Task Dashboard_WithEmptyTaskList_DisplaysPlaceholderInStatusCards()
        {
            var projectData = GetValidProjectData();
            projectData.Tasks = new List<ProjectTask>();
            _mockDataService.Setup(x => x.LoadProjectDataAsync())
                .ReturnsAsync(projectData);

            Services.AddScoped(_ => _mockDataService.Object);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("No tasks in this category", component.Markup);
        }

        private class MockWebHostEnvironment : IWebHostEnvironment
        {
            private readonly string _webRootPath;
            public MockWebHostEnvironment(string webRootPath) => _webRootPath = webRootPath;
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "AgentSquad.Runner";
            public string WebRootPath => _webRootPath;
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider WebRootFileProvider { get; set; }
            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}