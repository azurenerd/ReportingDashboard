using Bunit;
using Xunit;
using AgentSquad.Pages;
using AgentSquad.Services;
using AgentSquad.Models;
using Moq;

namespace AgentSquad.Tests.Acceptance
{
    public class ResponsiveDesignAcceptanceTests : TestContext
    {
        private readonly Mock<ProjectDataService> _mockProjectDataService;
        private ProjectData _testProjectData;

        public ResponsiveDesignAcceptanceTests()
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
                    Name = "Q2 Mobile App Release",
                    Description = "Release mobile app v2.0",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 6, 30)
                },
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Phase 1: Design", Status = MilestoneStatus.Completed, CompletionPercentage = 100, TargetDate = new DateTime(2026, 1, 15) },
                    new Milestone { Name = "Phase 2: Development", Status = MilestoneStatus.InProgress, CompletionPercentage = 60, TargetDate = new DateTime(2026, 4, 1) },
                    new Milestone { Name = "Phase 3: Testing", Status = MilestoneStatus.Pending, CompletionPercentage = 0, TargetDate = new DateTime(2026, 5, 15) },
                    new Milestone { Name = "Phase 4: Launch", Status = MilestoneStatus.Pending, CompletionPercentage = 0, TargetDate = new DateTime(2026, 6, 30) }
                },
                Tasks = new List<TaskItem>
                {
                    new TaskItem { Id = "T1", Name = "Dashboard UI", Status = TaskStatus.Shipped, AssignedTo = "Engineer 1", DueDate = new DateTime(2026, 1, 15) },
                    new TaskItem { Id = "T2", Name = "Data Load Service", Status = TaskStatus.Shipped, AssignedTo = "Engineer 2", DueDate = new DateTime(2026, 1, 30) },
                    new TaskItem { Id = "T3", Name = "Error Handling", Status = TaskStatus.Shipped, AssignedTo = "Engineer 1", DueDate = new DateTime(2026, 2, 1) },
                    new TaskItem { Id = "T4", Name = "Timeline Component", Status = TaskStatus.InProgress, AssignedTo = "Engineer 3", DueDate = new DateTime(2026, 4, 1) },
                    new TaskItem { Id = "T5", Name = "Progress Metrics", Status = TaskStatus.InProgress, AssignedTo = "Engineer 2", DueDate = new DateTime(2026, 4, 15) },
                    new TaskItem { Id = "T6", Name = "Responsive Grid", Status = TaskStatus.InProgress, AssignedTo = "Engineer 1", DueDate = new DateTime(2026, 4, 30) },
                    new TaskItem { Id = "T7", Name = "Advanced Analytics", Status = TaskStatus.CarriedOver, AssignedTo = "Engineer 4", DueDate = new DateTime(2026, 5, 1) },
                    new TaskItem { Id = "T8", Name = "Multi-Project Support", Status = TaskStatus.CarriedOver, AssignedTo = "Engineer 3", DueDate = new DateTime(2026, 5, 15) }
                },
                Metrics = new Metrics
                {
                    CompletionPercentage = 68,
                    ShippedCount = 3,
                    InProgressCount = 3,
                    CarriedOverCount = 2,
                    BurndownRate = 1.2m
                }
            };
        }

        [Fact]
        public async Task AC1_BootstrapGridFullyIntegrated()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("container-fluid", component.Markup);
            Assert.Contains("row", component.Markup);
            Assert.Contains("col-", component.Markup);
        }

        [Fact]
        public async Task AC2_MediaQueriesDefinedForAllBreakpoints()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var markup = component.Markup;

            Assert.Contains("col-md-", markup);
            Assert.Contains("col-lg-", markup);
        }

        [Fact]
        public async Task AC3_FontSizingScale_BaseSixteen()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("fs-1", component.Markup);
            Assert.Contains("fs-2", component.Markup);
            Assert.Contains("fs-3", component.Markup);
        }

        [Fact]
        public async Task AC4_PaddingMarginUtilitiesDefined()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("section-spacing", component.Markup);
            Assert.Contains("card-spacing", component.Markup);
            Assert.Contains("component-gutter", component.Markup);
        }

        [Fact]
        public async Task AC5_StatusCards_ResponsiveGridCollapse()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var markup = component.Markup;
            Assert.Contains("col-12", markup);
            Assert.Contains("col-md-6", markup);
            Assert.Contains("col-lg-4", markup);
        }

        [Fact]
        public async Task AC6_MilestoneTimeline_FullWidthResponsive()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("section-spacing", component.Markup);
            Assert.Contains("timeline", component.Markup.ToLower());
        }

        [Fact]
        public async Task AC7_ProgressMetrics_FullWidthResponsive()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Progress Metrics", component.Markup);
        }

        [Fact]
        public async Task AC8_NoAnimationsForScreenshots()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var markup = component.Markup.ToLower();

            Assert.DoesNotContain("@keyframes", markup);
            Assert.DoesNotContain("animation:", markup);
            Assert.DoesNotContain("transition:", markup);
        }

        [Fact]
        public async Task AC9_NoLayoutShiftsDuringLoad()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var rows = component.FindAll(".row");
            foreach (var row in rows)
            {
                var children = row.QuerySelectorAll("[class*='col-']");
                Assert.NotEmpty(children);
            }
        }

        [Fact]
        public async Task AC10_ConsistentSpacingHierarchy()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var markup = component.Markup;

            Assert.Contains("section-spacing", markup);
            Assert.Contains("card-spacing", markup);
            Assert.Contains("component-gutter", markup);
        }

        [Fact]
        public async Task AC11_ResponsiveTypography()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var headings = component.FindAll("h1, h2, h3, h4");
            Assert.NotEmpty(headings);
        }

        [Fact]
        public async Task AC12_AllTextReadable()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var textElements = component.FindAll("h1, h2, h3, h4, h5, h6, p, span, li");
            foreach (var element in textElements)
            {
                var text = element.TextContent.Trim();
                Assert.NotEmpty(text);
            }
        }

        [Fact]
        public async Task AC13_CustomCSSFileCreated()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("dashboard.css", component.Markup);
        }
    }
}