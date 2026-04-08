using Bunit;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components
{
    public class DashboardTests : TestContext
    {
        private readonly Mock<ProjectDataService> _projectDataServiceMock;

        public DashboardTests()
        {
            _projectDataServiceMock = new Mock<ProjectDataService>(MockBehavior.Strict, 
                new Mock<ILogger<ProjectDataService>>().Object);
        }

        [Fact]
        public void Dashboard_Renders_WithoutParameters()
        {
            // Arrange
            Services.AddScoped(_ => _projectDataServiceMock.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            Assert.NotNull(component);
            component.MarkupMatches(@"<div class=""container-fluid mt-5"">*</div>");
        }

        [Fact]
        public void Dashboard_DisplaysHeading()
        {
            // Arrange
            Services.AddScoped(_ => _projectDataServiceMock.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            component.Find("h1").TextContent.Should().Contain("Executive Dashboard");
        }

        [Fact]
        public void Dashboard_DisplaysPlaceholderText()
        {
            // Arrange
            Services.AddScoped(_ => _projectDataServiceMock.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            component.Markup.Should().Contain("Dashboard placeholder");
        }

        [Fact]
        public void Dashboard_RendersContainer_WithCorrectClasses()
        {
            // Arrange
            Services.AddScoped(_ => _projectDataServiceMock.Object);

            // Act
            var component = RenderComponent<Dashboard>();
            var container = component.Find(".container-fluid");

            // Assert
            Assert.NotNull(container);
            container.ClassList.Should().Contain("mt-5");
        }

        [Fact]
        public void Dashboard_RendersTextWithMutedClass()
        {
            // Arrange
            Services.AddScoped(_ => _projectDataServiceMock.Object);

            // Act
            var component = RenderComponent<Dashboard>();
            var paragraph = component.Find(".text-muted");

            // Assert
            Assert.NotNull(paragraph);
        }
    }
}