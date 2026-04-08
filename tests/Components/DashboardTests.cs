using Bunit;
using Xunit;
using Moq;
using AgentSquad.Components;
using AgentSquad.Models;
using AgentSquad.Services;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class DashboardTests : TestContext
    {
        public DashboardTests()
        {
            var mockService = new Mock<IProjectDataService>();
            mockService.Setup(s => s.LoadProjectDataAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProjectData { Milestones = new List<Milestone>(), Tasks = new List<ProjectTask>() });
            
            Services.AddScoped(_ => mockService.Object);
        }

        [Fact]
        public void Dashboard_RendersSuccessfully()
        {
            var component = RenderComponent<Dashboard>();
            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_InitializesWithData()
        {
            var component = RenderComponent<Dashboard>();
            var markup = component.Markup;
            Assert.False(string.IsNullOrEmpty(markup));
        }
    }
}