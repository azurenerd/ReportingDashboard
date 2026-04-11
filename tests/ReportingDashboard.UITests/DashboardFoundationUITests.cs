using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardFoundationUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardFoundationUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HomePage_LoadsSuccessfully_ReturnsHttp200()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_LoadsSuccessfully_Failed");
            throw;
        }
    }

    [Fact]
    public async Task HomePage_HasCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var title = await dashboardPage.GetPageTitleAsync();
            title.Should().Contain("Dashboard");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_HasCorrectTitle_Failed");
            throw;
        }
    }

    [Fact]
    public async Task HomePage_IncludesDashboardCss()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var cssLink = page.Locator("link[href='css/dashboard.css']");
            await Assertions.Expect(cssLink).ToHaveCountAsync(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_IncludesDashboardCss_Failed");
            throw;
        }
    }

    [Fact]
    public async Task HomePage_IncludesBlazorServerScript()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var script = page.Locator("script[src='_framework/blazor.server.js']");
            await Assertions.Expect(script).ToHaveCountAsync(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_IncludesBlazorScript_Failed");
            throw;
        }
    }

    [Fact]
    public async Task HomePage_HasNoDefaultBlazorNavSidebar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var navMenu = page.Locator(".sidebar, .nav-menu, nav.navbar, .top-row");
            var count = await navMenu.CountAsync();
            count.Should().Be(0, "dashboard should have no Blazor default navigation chrome");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_NoBlazorNav_Failed");
            throw;
        }
    }

    [Fact]
    public async Task HomePage_HasNoDefaultBlazorFooter()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var footer = page.Locator("footer");
            var count = await footer.CountAsync();
            count.Should().Be(0, "dashboard should have no footer elements");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_NoFooter_Failed");
            throw;
        }
    }

    [Fact]
    public async Task HomePage_ViewportIs1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var metaViewport = page.Locator("meta[name='viewport']");
            var content = await metaViewport.GetAttributeAsync("content");
            content.Should().Contain("1920");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HomePage_Viewport_Failed");
            throw;
        }
    }

    [Fact]
    public async Task CssStylesheet_LoadsSuccessfully()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");

            response.Should().NotBeNull();
            response!.Status.Should().Be(200);

            var body = await response.TextAsync();
            body.Should().NotBeEmpty();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "CssStylesheet_Loads_Failed");
            throw;
        }
    }
}