using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using AgentSquad.Runner.UITests.Pages;

namespace AgentSquad.Runner.UITests
{
    [TestFixture]
    [Category("UI")]
    public class MainLayoutTests : PlaywrightFixture
    {
        private DashboardPage? _dashboardPage;

        [SetUp]
        public void TestSetup()
        {
            if (Page != null)
                _dashboardPage = new DashboardPage(Page, BaseUrl);
        }

        [Test]
        public async Task MainLayout_ShouldRenderWithoutCrashing()
        {
            // Arrange
            Assert.NotNull(_dashboardPage, "Dashboard page should be initialized");

            // Act
            await _dashboardPage!.NavigateToDashboardAsync();

            // Assert - verify layout renders
            var container = await Page!.Locator(".dashboard-container").CountAsync();
            Assert.That(container, Is.GreaterThan(0), 
                "MainLayout should render dashboard container");

            // Assert - verify no console errors from layout initialization
            var errorLogged = false;
            Page.Console += (sender, e) =>
            {
                if (e.Type == "error")
                    errorLogged = true;
            };

            await Task.Delay(500);
            Assert.That(errorLogged, Is.False, 
                "MainLayout should not cause console errors");

            // Assert - verify CSS is loaded
            var cssLoaded = await Page.EvaluateAsync<bool>(
                @"() => {
                    const links = document.querySelectorAll('link[rel=stylesheet]');
                    return links.length > 0;
                }"
            );
            Assert.That(cssLoaded, Is.True, 
                "MainLayout should load CSS stylesheets");
        }
    }
}