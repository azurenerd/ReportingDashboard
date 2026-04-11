using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Verifies that all CSS classes from the OriginalDesignConcept.html exist
/// in the loaded dashboard.css and are applied correctly to elements.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class CssClassesUITests
{
    private readonly PlaywrightFixture _fixture;

    public CssClassesUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CssFile_ReturnsHttp200()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            response.Should().NotBeNull();
            response!.Status.Should().Be(200);

            var contentType = response.Headers.GetValueOrDefault("content-type", "");
            contentType.Should().Contain("css");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "CssFile_Http200");
            throw;
        }
    }

    [Fact]
    public async Task CssFile_ContainsAllRequiredClasses()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var cssContent = await response!.TextAsync();

            var requiredClasses = new[]
            {
                ".hdr", ".sub", ".tl-area", ".tl-svg-box",
                ".hm-wrap", ".hm-title", ".hm-grid", ".hm-corner",
                ".hm-col-hdr", ".hm-row-hdr", ".hm-cell", ".it",
                ".ship-hdr", ".ship-cell", ".prog-hdr", ".prog-cell",
                ".carry-hdr", ".carry-cell", ".block-hdr", ".block-cell",
                ".apr-hdr"
            };

            foreach (var cls in requiredClasses)
            {
                cssContent.Should().Contain(cls, $"CSS must contain class {cls}");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "CssFile_RequiredClasses");
            throw;
        }
    }

    [Fact]
    public async Task CssFile_BodyRules_AreCorrect()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain("1920px", "body width should be 1920px");
            css.Should().Contain("1080px", "body height should be 1080px");
            // Check for overflow hidden with or without space
            var hasOverflowHidden = css.Contains("overflow:hidden") || css.Contains("overflow: hidden");
            hasOverflowHidden.Should().BeTrue("CSS should contain overflow:hidden or overflow: hidden");
            css.Should().Contain("Segoe UI");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "CssFile_BodyRules");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HdrClass_IsApplied()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hdrCount = await page.Locator(".hdr").CountAsync();
            hdrCount.Should().BeGreaterThanOrEqualTo(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_HdrApplied");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_TlAreaClass_IsApplied()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var tlCount = await page.Locator(".tl-area").CountAsync();
            tlCount.Should().BeGreaterThanOrEqualTo(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_TlAreaApplied");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HmWrapClass_IsApplied()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hmCount = await page.Locator(".hm-wrap").CountAsync();
            hmCount.Should().BeGreaterThanOrEqualTo(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_HmWrapApplied");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HmGridClass_IsApplied()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var gridCount = await page.Locator(".hm-grid").CountAsync();
            gridCount.Should().BeGreaterThanOrEqualTo(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_HmGridApplied");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_NoBlazorDefaultCss_IsApplied()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            // Ensure no default Blazor chrome classes exist
            var topRow = await page.Locator(".top-row").CountAsync();
            var mainLayout = await page.Locator(".main-layout").CountAsync();
            var navMenu = await page.Locator(".nav-menu").CountAsync();

            topRow.Should().Be(0, "no default Blazor top-row");
            mainLayout.Should().Be(0, "no default Blazor main-layout");
            navMenu.Should().Be(0, "no default Blazor nav-menu");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_NoBlazorDefault");
            throw;
        }
    }
}