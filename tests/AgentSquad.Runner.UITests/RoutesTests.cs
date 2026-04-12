using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using AgentSquad.Runner.UITests.Pages;

namespace AgentSquad.Runner.UITests
{
    [TestFixture]
    [Category("UI")]
    public class RoutesTests : PlaywrightFixture
    {
        private DashboardPage? _dashboardPage;

        [SetUp]
        public void TestSetup()
        {
            if (Page != null)
                _dashboardPage = new DashboardPage(Page, BaseUrl);
        }

        [Test]
        public async Task Routes_DefaultRouteShould_RenderMainLayout()
        {
            // Arrange
            Assert.NotNull(_dashboardPage, "Dashboard page should be initialized");

            // Act
            await _dashboardPage!.NavigateToDashboardAsync();
            await Task.Delay(1000); // Allow page to settle

            // Assert - verify main layout is loaded
            var container = Page!.Locator(".dashboard-container");
            var isVisible = await container.IsVisibleAsync();
            Assert.That(isVisible, Is.True, 
                "MainLayout should render dashboard container");

            // Assert - verify no JavaScript errors occurred
            var jsErrors = new System.Collections.Generic.List<string>();
            Page.Console += (sender, e) =>
            {
                if (e.Type == "error")
                    jsErrors.Add(e.Text);
            };

            await Task.Delay(500); // Monitor for errors
            Assert.That(jsErrors, Is.Empty, 
                "Should not have JavaScript console errors");
        }
    }
}