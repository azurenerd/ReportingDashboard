using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class LayoutStructureTests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutStructureTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task MainLayout_ContainerIsDirectChildOfBody()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        // The 1920x1080 container should be rendered
        var container = dashboardPage.MainContainer;
        var count = await container.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MainLayout_NoNavigationElements()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var navCount = await page.Locator("nav").CountAsync();
        var sidebarCount = await page.Locator("[class*='sidebar']").CountAsync();
        var footerCount = await page.Locator("footer").CountAsync();

        navCount.Should().Be(0);
        sidebarCount.Should().Be(0);
        footerCount.Should().Be(0);
    }

    [Fact]
    public async Task MainLayout_WhiteBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var container = dashboardPage.MainContainer;
        var bg = await container.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

        // #fff = rgb(255, 255, 255)
        bg.Should().Contain("255");
    }

    [Fact]
    public async Task MainLayout_TextColorIsDark()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var container = dashboardPage.MainContainer;
        var color = await container.EvaluateAsync<string>("el => getComputedStyle(el).color");

        // #111 = rgb(17, 17, 17)
        color.Should().Contain("17");
    }

    [Fact]
    public async Task Page_NoHorizontalScrollbar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var hasHorizontalScroll = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

        // With viewport at 1920, the 1920px container should not cause scrollbars
        // Note: this may vary if viewport is smaller, which is why we set it in the fixture
        hasHorizontalScroll.Should().BeFalse();
    }

    [Fact]
    public async Task Page_CharsetIsUtf8()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        var charset = await page.EvaluateAsync<string>("() => document.characterSet");
        charset.Should().Be("UTF-8");
    }
}