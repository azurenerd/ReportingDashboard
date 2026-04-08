using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using AgentSquad.Services;

namespace AgentSquad.Tests.Components
{
    public class DashboardComponentTests : TestContext
    {
        private readonly ProjectDataService _projectDataService;

        public DashboardComponentTests()
        {
            _projectDataService = new ProjectDataService();
        }

        [Fact]
        public void DashboardComponent_Renders()
        {
            Services.AddScoped(_ => _projectDataService);

            var component = RenderComponent<Dashboard>();

            Assert.NotNull(component);
        }

        [Fact]
        public void DashboardComponent_DisplaysTitle()
        {
            Services.AddScoped(_ => _projectDataService);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Dashboard", component.Markup);
        }

        [Fact]
        public void DashboardComponent_ContainsProjectSection()
        {
            Services.AddScoped(_ => _projectDataService);

            var component = RenderComponent<Dashboard>();

            Assert.NotEmpty(component.Markup);
        }
    }
}