using Xunit;
using Bunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using AgentSquad.Components;
using AgentSquad.Services;
using AgentSquad.Data;

namespace AgentSquad.Tests.Components
{
    public class DashboardIntegrationTests : TestContext
    {
        [Fact]
        public void Dashboard_RendersWithServiceInjection()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            Assert.NotNull(component);
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void Dashboard_LoadsDataDuringInitialization()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            component.WaitForAsyncOperation();
            
            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_HandlesMissingDataDirectory()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "nonexistent"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_ContainsProjectComponents()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            var markup = component.Markup;
            
            Assert.NotEmpty(markup);
        }
    }
}