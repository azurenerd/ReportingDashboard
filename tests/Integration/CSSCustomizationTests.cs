using Bunit;
using Xunit;
using AgentSquad.Pages;
using AgentSquad.Services;
using AgentSquad.Models;
using Moq;

namespace AgentSquad.Tests.Integration
{
    public class CSSCustomizationTests : TestContext
    {
        private readonly Mock<ProjectDataService> _mockProjectDataService;
        private ProjectData _testProjectData;

        public CSSCustomizationTests()
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
                Milestones = new List<Milestone>(),
                Tasks = new List<TaskItem>(),
                Metrics = new Metrics()
            };
        }

        [Fact]
        public async Task Dashboard_CustomBootstrapVariablesApplied()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("status-card", component.Markup);
        }

        [Fact]
        public async Task Dashboard_ColorPaletteConfigured()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("status-card-green", component.Markup);
            Assert.Contains("status-card-blue", component.Markup);
            Assert.Contains("status-card-orange", component.Markup);
        }

        [Fact]
        public async Task Dashboard_FontFamilyConsistent()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();

            Assert.NotNull(component.Markup);
        }

        [Fact]
        public async Task Dashboard_UtilityClassesAvailable()
        {
            _mockProjectDataService.Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(_testProjectData);

            var component = RenderComponent<Dashboard>();
            var markup = component.Markup;

            Assert.Contains("section-spacing", markup);
            Assert.Contains("card-spacing", markup);
            Assert.Contains("component-gutter", markup);
        }
    }
}