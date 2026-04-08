using Xunit;
using Bunit;
using Moq;
using Microsoft.AspNetCore.Hosting;
using AgentSquad.Components;
using AgentSquad.Services;
using AgentSquad.Data;

namespace AgentSquad.Tests.Acceptance
{
    public class DashboardAcceptanceTests : TestContext
    {
        [Fact]
        public void Dashboard_LoadsAndDisplaysProjectDataViaService()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            component.WaitForAsyncOperation();

            Assert.NotNull(component);
            Assert.NotEmpty(component.Markup);
        }

        [Fact]
        public void Dashboard_HandlesMissingDataGracefully()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "nonexistent"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            component.WaitForAsyncOperation();

            Assert.NotNull(component);
        }

        [Fact]
        public void Dashboard_InitializesWithProjectService()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            
            Assert.NotNull(component);
            Assert.IsType<Dashboard>(component.Instance);
        }

        [Fact]
        public void Dashboard_RefreshesDataOnComponentLoad()
        {
            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            
            var service = new ProjectDataService(mockEnvironment.Object);
            Services.AddScoped(_ => service);

            var component = RenderComponent<Dashboard>();
            component.WaitForAsyncOperation();

            var cachedData = service.GetCachedData();
            Assert.NotNull(cachedData);
        }
    }
}