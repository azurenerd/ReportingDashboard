using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardLayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardLayoutUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_NoNavSidebar_Present()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hasSidebar = await dashboard.HasNavSidebarAsync();
            hasSidebar.Should().BeFalse("dashboard should have no Blazor default nav sidebar");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_NoNavSidebar");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_NoFooter_Present()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hasFooter = await dashboard.HasFooterAsync();
            hasFooter.Should().BeFalse("dashboard should have no footer element");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_NoFooter");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_BodyOverflow_IsHidden()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var overflow = await page.EvalOnSelectorAsync<string>("body",
                "el => window.getComputedStyle(el).overflow");
            overflow.Should().Be("hidden");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_BodyOverflow");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_BodyWidth_Is1920()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var width = await page.EvalOnSelectorAsync<string>("body",
                "el => window.getComputedStyle(el).width");
            width.Should().Be("1920px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_BodyWidth");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_BodyHeight_Is1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var height = await page.EvalOnSelectorAsync<string>("body",
                "el => window.getComputedStyle(el).height");
            height.Should().Be("1080px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_BodyHeight");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_FontFamily_IncludesSegoeUI()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var fontFamily = await page.EvalOnSelectorAsync<string>("body",
                "el => window.getComputedStyle(el).fontFamily");
            fontFamily.Should().ContainAny("Segoe UI", "\"Segoe UI\"", "'Segoe UI'");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_FontFamily");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_BodyBackground_IsWhite()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await page.EvalOnSelectorAsync<string>("body",
                "el => window.getComputedStyle(el).backgroundColor");
            // rgb(255, 255, 255) or rgba(0,0,0,0) depending on CSS
            bg.Should().Match(v =>
                v.Contains("255, 255, 255") || v == "rgb(255, 255, 255)" || v == "rgba(0, 0, 0, 0)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_BodyBackground");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_FlexDirection_IsColumn()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var flexDir = await page.EvalOnSelectorAsync<string>("body",
                "el => window.getComputedStyle(el).flexDirection");
            flexDir.Should().Be("column");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_FlexDirection");
            throw;
        }
    }
}