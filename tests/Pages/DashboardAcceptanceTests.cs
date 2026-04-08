using Bunit;
using Xunit;
using AgentSquad.Runner.Pages;
using AgentSquad.Runner.Components;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Pages
{
    public class DashboardAcceptanceTests : TestContext
    {
        [Fact]
        public async Task Dashboard_DisplaysExecutiveDashboardTitle()
        {
            var cut = RenderComponent<Dashboard>();
            await cut.InvokeAsync(async () => await Task.Delay(600));

            var heading = cut.Find("h1");
            Assert.Contains("Executive Dashboard", heading.TextContent);
        }

        [Fact]
        public async Task Dashboard_ShowsLoadingIndicator_Initially()
        {
            var cut = RenderComponent<Dashboard>();

            var spinner = cut.QuerySelector(".spinner-border");
            Assert.NotNull(spinner);
        }

        [Fact]
        public async Task Dashboard_RendersProjectName_AfterLoading()
        {
            var cut = RenderComponent<Dashboard>();
            await cut.InvokeAsync(async () => await Task.Delay(600));

            var projectHeading = cut.Find("h2");
            Assert.Contains("Sample Executive Project", projectHeading.TextContent);
        }

        [Fact]
        public async Task Dashboard_ShowsStatusPageDescription()
        {
            var cut = RenderComponent<Dashboard>();
            await cut.InvokeAsync(async () => await Task.Delay(600));

            var description = cut.Find(".text-muted");
            Assert.Contains("Status page for executive reporting", description.TextContent);
        }

        [Fact]
        public void Dashboard_IsProtectedByErrorBoundary()
        {
            var cut = RenderComponent<Dashboard>();

            var errorBoundary = cut.FindComponent<ErrorBoundary>();
            Assert.NotNull(errorBoundary);
        }

        [Fact]
        public void Dashboard_UsesBootstrapResponsiveGrid()
        {
            var cut = RenderComponent<Dashboard>();

            var containerFluid = cut.QuerySelector(".container-fluid");
            Assert.NotNull(containerFluid);

            var rows = cut.QuerySelectorAll(".row");
            Assert.NotEmpty(rows);
        }

        [Fact]
        public async Task Dashboard_ResponsiveLayoutStructure()
        {
            var cut = RenderComponent<Dashboard>();
            await cut.InvokeAsync(async () => await Task.Delay(600));

            var columns = cut.QuerySelectorAll(".col-12");
            Assert.NotEmpty(columns);
        }

        [Fact]
        public void Dashboard_HasMinimumHeightForFullScreen()
        {
            var cut = RenderComponent<Dashboard>();

            var container = cut.Find(".dashboard-container");
            var minHeight = container.Style["min-height"];
            Assert.NotEmpty(minHeight);
        }

        [Fact]
        public void Dashboard_UsesProperBackgroundColor()
        {
            var cut = RenderComponent<Dashboard>();

            var container = cut.Find(".dashboard-container");
            var bgColor = container.Style["background-color"];
            Assert.NotNull(bgColor);
        }

        [Fact]
        public async Task Dashboard_LoadingStateBehavior()
        {
            var cut = RenderComponent<Dashboard>();

            // Initially loading
            var initialSpinner = cut.QuerySelector(".spinner-border");
            Assert.NotNull(initialSpinner);

            // After initialization
            await cut.InvokeAsync(async () => await Task.Delay(600));

            var finalSpinner = cut.QuerySelector(".spinner-border");
            Assert.Null(finalSpinner);

            var content = cut.Find("h2");
            Assert.NotNull(content);
        }

        [Fact]
        public async Task Dashboard_FullInitializationCycle()
        {
            var cut = RenderComponent<Dashboard>();

            // Verify ErrorBoundary wraps content
            var errorBoundary = cut.FindComponent<ErrorBoundary>();
            Assert.NotNull(errorBoundary);

            // Verify loading state
            Assert.NotNull(cut.QuerySelector(".spinner-border"));

            // Wait for data load
            await cut.InvokeAsync(async () => await Task.Delay(600));

            // Verify content displays
            var title = cut.Find("h1");
            var projectName = cut.Find("h2");
            var description = cut.Find(".text-muted");

            Assert.Contains("Executive Dashboard", title.TextContent);
            Assert.Contains("Sample Executive Project", projectName.TextContent);
            Assert.Contains("Status page for executive reporting", description.TextContent);
        }
    }
}