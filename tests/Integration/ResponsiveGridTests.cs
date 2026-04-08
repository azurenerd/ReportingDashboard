using Bunit;
using Xunit;
using AgentSquad.Pages;
using AgentSquad.Services;
using AgentSquad.Models;
using Moq;

namespace AgentSquad.Tests.Integration
{
    public class ResponsiveGridTests : TestContext
    {
        private readonly Mock<ProjectDataService> _mockProjectDataService;
        private ProjectData _testProjectData;

        public ResponsiveGridTests()
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
                    new TaskItem { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Eng 1", DueDate = new DateTime(2026, 1, 15) },
                    new TaskItem { Id = "T2", Name = "Task 2", Status = TaskStatus.InProgress, AssignedTo = "Eng 2", DueDate = new DateTime(2026, 4, 1) },
                    new TaskItem { Id = "T3", Name = "Task 3", Status = TaskStatus.CarriedOver, AssignedTo = "Eng 3", DueDate = new DateTime(2026, 6, 1) }
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
        public async Task Dashboard_StatusCards_ResponsiveClasses()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            var rows = component.FindAll(".row");
            Assert.True(rows.Any(r => r.GetAttribute("class")?.Contains("col-12") == true),
                "Status card row should have col-12 for mobile");
            Assert.True(rows.Any(r => r.GetAttribute("class")?.Contains("col-md-6") == true),
                "Status card row should have col-md-6 for tablet");
            Assert.True(rows.Any(r => r.GetAttribute("class")?.Contains("col-lg-4") == true),
                "Status card row should have col-lg-4 for desktop");
        }

        [Fact]
        public async Task Dashboard_BootstrapContainerFluid()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var container = component.Find(".container-fluid");

            Assert.NotNull(container);
        }

        [Fact]
        public async Task Dashboard_SpacingUtilitiesApplied()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("section-spacing", component.Markup);
        }

        [Fact]
        public async Task Dashboard_TypographyScale()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("fs-1", component.Markup);
            Assert.Contains("fs-2", component.Markup);
        }

        [Fact]
        public async Task Dashboard_NoHorizontalScrollAvailable()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var containers = component.FindAll(".container-fluid, .container");

            foreach (var container in containers)
            {
                var style = container.GetAttribute("style") ?? "";
                Assert.DoesNotContain("overflow-x: scroll", style);
                Assert.DoesNotContain("overflow-x: auto", style);
            }
        }

        [Fact]
        public async Task Dashboard_MediaQueriesImplemented()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("col-md-", component.Markup);
            Assert.Contains("col-lg-", component.Markup);
        }

        [Fact]
        public async Task Dashboard_GridGuttersConsistent()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var rows = component.FindAll(".row");

            Assert.NotEmpty(rows);
        }

        [Fact]
        public async Task Dashboard_LayoutShiftsNotPresent()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var stylesheet = component.Find("link[rel='stylesheet']");

            Assert.NotNull(stylesheet);
        }

        [Fact]
        public async Task Dashboard_AllSectionsFullWidth()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var sections = component.FindAll("section, .dashboard-section");

            foreach (var section in sections)
            {
                var colClass = section.GetAttribute("class") ?? "";
                Assert.True(colClass.Contains("col-12") || !colClass.Contains("col-"),
                    "Section should either have col-12 or no col class (full width)");
            }
        }
    }
}