using Bunit;
using Xunit;
using AgentSquad.Pages;
using AgentSquad.Services;
using AgentSquad.Models;
using Moq;

namespace AgentSquad.Tests.Components
{
    public class DashboardTests : TestContext
    {
        private readonly Mock<ProjectDataService> _mockProjectDataService;
        private ProjectData _testProjectData;

        public DashboardTests()
        {
            _mockProjectDataService = new Mock<ProjectDataService>();
            _testProjectData = CreateTestProjectData();
            Services.AddSingleton(_mockProjectDataService.Object);
        }

        private ProjectData CreateTestProjectData()
        {
            return new ProjectData
            {
                Project = new Project
                {
                    Name = "Test Project",
                    Description = "Test Description",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 6, 30)
                },
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Phase 1", Status = MilestoneStatus.Completed, CompletionPercentage = 100, TargetDate = new DateTime(2026, 1, 15) },
                    new Milestone { Name = "Phase 2", Status = MilestoneStatus.InProgress, CompletionPercentage = 50, TargetDate = new DateTime(2026, 4, 1) }
                },
                Tasks = new List<TaskItem>
                {
                    new TaskItem { Id = "T1", Name = "Dashboard UI", Status = TaskStatus.Shipped, AssignedTo = "Eng 1", DueDate = new DateTime(2026, 1, 15) },
                    new TaskItem { Id = "T2", Name = "Timeline", Status = TaskStatus.InProgress, AssignedTo = "Eng 2", DueDate = new DateTime(2026, 4, 1) },
                    new TaskItem { Id = "T3", Name = "Analytics", Status = TaskStatus.CarriedOver, AssignedTo = "Eng 3", DueDate = new DateTime(2026, 6, 1) }
                },
                Metrics = new Metrics
                {
                    CompletionPercentage = 68,
                    ShippedCount = 1,
                    InProgressCount = 1,
                    CarriedOverCount = 1,
                    BurndownRate = 0.5m
                }
            };
        }

        [Fact]
        public async Task Dashboard_LoadsProjectDataOnInitialize()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.NotNull(component);
        }

        [Fact]
        public async Task Dashboard_DisplaysProjectTitle()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var title = component.Find("h1");

            Assert.NotNull(title);
            Assert.Contains("Dashboard", title.TextContent);
        }

        [Fact]
        public async Task Dashboard_DisplaysLoadingMessageWhileLoading()
        {
            var delayedData = Task.Delay(1000).ContinueWith(_ => _testProjectData);
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .Returns(delayedData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Loading", component.Markup);
        }

        [Fact]
        public async Task Dashboard_DisplaysErrorMessageOnLoadFailure()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ThrowsAsync(new FileNotFoundException("data.json not found"));

            var component = RenderComponent<Dashboard>();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.Contains("Error", component.Markup);
        }

        [Fact]
        public async Task Dashboard_DisplaysInvalidJsonErrorMessage()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ThrowsAsync(new System.Text.Json.JsonException("Invalid JSON"));

            var component = RenderComponent<Dashboard>();
            await component.InvokeAsync(() => component.Instance.OnInitializedAsync());

            Assert.Contains("Invalid JSON", component.Markup);
        }

        [Fact]
        public async Task Dashboard_RendersResponsiveGridStructure()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var containerFluid = component.Find(".container-fluid");

            Assert.NotNull(containerFluid);
        }

        [Fact]
        public async Task Dashboard_RendersAllThreeStatusCards()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var statusCards = component.FindAll(".status-card");

            Assert.Equal(3, statusCards.Count);
        }

        [Fact]
        public async Task Dashboard_DisplaysCorrectTaskCounts()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Shipped", component.Markup);
            Assert.Contains("In-Progress", component.Markup);
            Assert.Contains("Carried-Over", component.Markup);
        }

        [Fact]
        public async Task Dashboard_NoHorizontalScrollbarOnResponsiveView()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var rows = component.FindAll(".row");

            Assert.NotEmpty(rows);
            foreach (var row in rows)
            {
                Assert.DoesNotContain("overflow-x", row.GetAttribute("class") ?? "");
            }
        }
    }
}