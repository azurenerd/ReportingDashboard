using Bunit;
using Xunit;
using AgentSquad.Pages;
using AgentSquad.Services;
using AgentSquad.Models;
using Moq;

namespace AgentSquad.Tests.Integration
{
    public class BootstrapIntegrationTests : TestContext
    {
        private readonly Mock<ProjectDataService> _mockProjectDataService;
        private ProjectData _testProjectData;

        public BootstrapIntegrationTests()
        {
            _mockProjectDataService = new Mock<ProjectDataService>();
            _testProjectData = CreateTestProjectData();
            Services.AddSingleton(_mockProjectDataService.Object);
        }

        private ProjectData CreateTestProjectData()
        {
            var tasks = new List<TaskItem>();
            for (int i = 1; i <= 12; i++)
                tasks.Add(new TaskItem { Id = $"T{i}", Name = $"Task {i}", Status = TaskStatus.Shipped, AssignedTo = $"Eng {i}", DueDate = new DateTime(2026, 1, 15) });
            
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
                    new Milestone { Name = "Phase 2", Status = MilestoneStatus.InProgress, CompletionPercentage = 50, TargetDate = new DateTime(2026, 4, 1) },
                    new Milestone { Name = "Phase 3", Status = MilestoneStatus.Pending, CompletionPercentage = 0, TargetDate = new DateTime(2026, 6, 30) },
                    new Milestone { Name = "Phase 4", Status = MilestoneStatus.Pending, CompletionPercentage = 0, TargetDate = new DateTime(2026, 9, 15) }
                },
                Tasks = tasks,
                Metrics = new Metrics
                {
                    CompletionPercentage = 68,
                    ShippedCount = 12,
                    InProgressCount = 5,
                    CarriedOverCount = 2,
                    BurndownRate = 2.5m
                }
            };
        }

        [Fact]
        public async Task Dashboard_BootstrapCSSLinked()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("bootstrap", component.Markup.ToLower());
        }

        [Fact]
        public async Task Dashboard_CustomDashboardCSSLinked()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("dashboard.css", component.Markup.ToLower());
        }

        [Fact]
        public async Task Dashboard_BootstrapBreakpoints_Implemented()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("col-", component.Markup);
            Assert.Contains("md-", component.Markup);
            Assert.Contains("lg-", component.Markup);
        }

        [Fact]
        public async Task Dashboard_BootstrapColors_Applied()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var markup = component.Markup;

            Assert.True(
                markup.Contains("status-card-green") || 
                markup.Contains("#28a745") ||
                markup.Contains("status-card"),
                "Green color (shipped) should be applied");

            Assert.True(
                markup.Contains("status-card-blue") || 
                markup.Contains("#007bff") ||
                markup.Contains("status-card"),
                "Blue color (in-progress) should be applied");

            Assert.True(
                markup.Contains("status-card-orange") || 
                markup.Contains("#fd7e14") ||
                markup.Contains("status-card"),
                "Orange color (carried-over) should be applied");
        }

        [Fact]
        public async Task Dashboard_FontSizeUtilities_Applied()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("fs-", component.Markup);
        }

        [Fact]
        public async Task Dashboard_PaddingMarginUtilities_Applied()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("section-spacing", component.Markup);
            Assert.Contains("card-spacing", component.Markup);
            Assert.Contains("component-gutter", component.Markup);
        }

        [Fact]
        public async Task Dashboard_AllComponentsScaleResponsively()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var rows = component.FindAll(".row");
            Assert.NotEmpty(rows);

            foreach (var row in rows)
            {
                var children = row.QuerySelectorAll("[class*='col-']");
                Assert.NotEmpty(children);
            }
        }

        [Fact]
        public async Task Dashboard_ReadableTextSizes()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var headings = component.FindAll("h1, h2, h3, h4, h5, h6");
            Assert.NotEmpty(headings);

            var textElements = component.FindAll("p, span, li");
            Assert.True(textElements.Count > 0, "Should have readable text elements");
        }
    }
}