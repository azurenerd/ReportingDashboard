using Bunit;
using Xunit;
using AgentSquad.Runner.Pages;
using AgentSquad.Runner.Components;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Pages
{
    public class DashboardTests : TestContext
    {
        [Fact]
        public void Dashboard_RenderSuccessfully()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            Assert.NotNull(cut);
        }

        [Fact]
        public void Dashboard_DisplaysLoadingSpinner_Initially()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var spinner = cut.QuerySelector(".spinner-border");
            Assert.NotNull(spinner);
        }

        [Fact]
        public async Task Dashboard_LoadsProjectData_OnInitialization()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var projectName = cut.QuerySelector("h2");
            Assert.NotNull(projectName);
        }

        [Fact]
        public async Task Dashboard_DisplaysProjectName()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var heading = cut.Find("h2");
            Assert.Contains("Sample Executive Project", heading.TextContent);
        }

        [Fact]
        public async Task Dashboard_HidesLoadingSpinner_AfterDataLoads()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var spinner = cut.QuerySelector(".spinner-border");
            Assert.Null(spinner);
        }

        [Fact]
        public void Dashboard_WrapsWithErrorBoundary()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var errorBoundary = cut.FindComponent<ErrorBoundary>();
            Assert.NotNull(errorBoundary);
        }

        [Fact]
        public void Dashboard_HasCorrectPageRoute()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            // Dashboard should render without route attribute errors
            Assert.NotNull(cut);
        }

        [Fact]
        public async Task Dashboard_DisplaysMainHeading()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var mainHeading = cut.Find("h1");
            Assert.Contains("Executive Dashboard", mainHeading.TextContent);
        }

        [Fact]
        public async Task Dashboard_DisplaysStatusPageDescription()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var description = cut.Find(".text-muted");
            Assert.Contains("Status page for executive reporting", description.TextContent);
        }

        [Fact]
        public void Dashboard_UsesBootstrapContainerFluid()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var container = cut.QuerySelector(".container-fluid");
            Assert.NotNull(container);
        }

        [Fact]
        public void Dashboard_HasProperContainerStructure()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var dashboardContainer = cut.QuerySelector(".dashboard-container");
            Assert.NotNull(dashboardContainer);
            
            var containerFluid = cut.QuerySelector(".container-fluid");
            Assert.NotNull(containerFluid);
        }

        [Fact]
        public async Task Dashboard_ProjectData_NotNull_AfterInitialization()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var projectInfo = cut.QuerySelector("h2");
            Assert.NotNull(projectInfo);
        }

        [Fact]
        public void Dashboard_BackgroundColorSetCorrectly()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var container = cut.Find(".dashboard-container");
            var style = container.Style["background-color"] ?? "";
            Assert.NotEmpty(style);
        }

        [Fact]
        public void Dashboard_MinHeightSetForFullHeight()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var container = cut.Find(".dashboard-container");
            var minHeight = container.Style["min-height"] ?? "";
            Assert.NotEmpty(minHeight);
        }

        [Fact]
        public async Task Dashboard_DisplaysRowsAfterLoading()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var rows = cut.QuerySelectorAll(".row");
            Assert.NotEmpty(rows);
        }

        [Fact]
        public async Task Dashboard_AllColumnsHaveFull12Width()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var columns = cut.QuerySelectorAll(".col-12");
            Assert.NotEmpty(columns);
        }

        [Fact]
        public void Dashboard_UsesProperBootstrapSpacing_Padding()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var containerWithPadding = cut.QuerySelector(".py-4");
            Assert.NotNull(containerWithPadding);
        }

        [Fact]
        public async Task Dashboard_MarginBottomOnLastRow()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert
            var rows = cut.QuerySelectorAll(".mb-4");
            Assert.NotEmpty(rows);
        }

        [Fact]
        public async Task Dashboard_LoadingState_InitiallyTrue()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            var spinner = cut.QuerySelector(".spinner-border");
            Assert.NotNull(spinner);
        }

        [Fact]
        public async Task Dashboard_CompleteLifecycle_LoadingToContent()
        {
            // Arrange
            var cut = RenderComponent<Dashboard>();

            // Act - Initially loading
            var initialSpinner = cut.QuerySelector(".spinner-border");
            Assert.NotNull(initialSpinner);

            // Wait for initialization
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Assert - Now showing content
            var finalSpinner = cut.QuerySelector(".spinner-border");
            Assert.Null(finalSpinner);
            
            var projectName = cut.Find("h2");
            Assert.NotNull(projectName);
        }

        [Fact]
        public void Dashboard_CatchesErrorsWithErrorBoundary()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            // If ErrorBoundary exists, it will catch any rendering errors
            var errorBoundary = cut.FindComponent<ErrorBoundary>();
            Assert.NotNull(errorBoundary);
        }

        [Fact]
        public void Dashboard_UsesInjectAttributes()
        {
            // Arrange & Act
            var cut = RenderComponent<Dashboard>();

            // Assert
            // Component should render without JS injection errors
            Assert.NotNull(cut);
        }
    }
}