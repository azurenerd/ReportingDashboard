using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Data;

namespace AgentSquad.Tests.Components
{
    public class DashboardIntegrationTests : TestContext
    {
        [Fact]
        public void Dashboard_RendersAllComponents()
        {
            var projectData = new ProjectData
            {
                Project = new ProjectInfo { Name = "TestProject" },
                Milestones = new List<Milestone>()
            };

            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.ProjectData, projectData));

            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_WithNullData_RendersWithoutCrash()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.ProjectData, new ProjectData()));

            Assert.NotNull(component);
        }
    }
}