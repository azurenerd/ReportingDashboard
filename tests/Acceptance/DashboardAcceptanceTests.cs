using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Data;

namespace AgentSquad.Tests.Acceptance
{
    public class DashboardAcceptanceTests : TestContext
    {
        [Fact]
        public void Dashboard_LoadsAndDisplaysProjectData()
        {
            var projectData = new ProjectData
            {
                Project = new ProjectInfo { Name = "Project Alpha" }
            };

            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.ProjectData, projectData));

            Assert.Contains("Project Alpha", component.Markup);
        }

        [Fact]
        public void Dashboard_HandlesMissingProjectData()
        {
            var component = RenderComponent<Dashboard>(parameters =>
                parameters.Add(p => p.ProjectData, new ProjectData { Project = new ProjectInfo() }));

            Assert.NotNull(component);
        }
    }
}