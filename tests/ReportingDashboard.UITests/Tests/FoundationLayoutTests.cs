using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the foundation layout from PR #539: 1920x1080 pixel-precise layout,
/// no Blazor default chrome, correct CSS, viewport meta, and static resources.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationLayoutTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationLayoutTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var title = await dashboard.GetPageTitleAsync();
            Assert.Contains("Executive", title);
            Assert.Contains("Dashboard", title);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasCorrectTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasViewportMetaWidth1920()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var count = await dashboard.ViewportMeta.CountAsync();
            Assert.True(count > 0, "Expected meta viewport with width=1920");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasViewportMetaWidth1920));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasBaseHref()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var count = await dashboard.BaseHref.CountAsync();
            Assert.True(count > 0, "Expected <base href=\"/\"> element");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasBaseHref));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasCssStylesheetLinked()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var count = await dashboard.CssLink.CountAsync();
            Assert.True(count > 0, "Expected link to css/dashboard.css");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasCssStylesheetLinked));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasBlazorScript()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var count = await dashboard.BlazorScript.CountAsync();
            Assert.True(count > 0, "Expected Blazor script reference");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasBlazorScript));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasExactWidth1920px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var width = await dashboard.GetBodyWidthAsync();
            Assert.Equal("1920px", width);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasExactWidth1920px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasExactHeight1080px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var height = await dashboard.GetBodyHeightAsync();
            Assert.Equal("1080px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasExactHeight1080px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasOverflowHidden()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var overflow = await dashboard.GetBodyOverflowAsync();
            Assert.Equal("hidden", overflow);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasOverflowHidden));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasSegoeUIFontFamily()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var fontFamily = await dashboard.GetBodyFontFamilyAsync();
            Assert.Contains("Segoe UI", fontFamily);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasSegoeUIFontFamily));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasWhiteBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var bgColor = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).backgroundColor");
            // White is rgb(255, 255, 255) or rgba(255, 255, 255, 1)
            Assert.Contains("255, 255, 255", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasWhiteBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasFlexColumnDisplay()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var display = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).display");
            var direction = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).flexDirection");
            Assert.Equal("flex", display);
            Assert.Equal("column", direction);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasFlexColumnDisplay));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NoBlazorDefaultChrome_NoNavSidebar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            Assert.False(await dashboard.HasNavSidebarAsync(),
                "Expected no Blazor default navigation sidebar, top bar, or footer");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NoBlazorDefaultChrome_NoNavSidebar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasHtmlLangAttribute()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var lang = await page.EvaluateAsync<string>(
                "() => document.documentElement.getAttribute('lang')");
            Assert.Equal("en", lang);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasHtmlLangAttribute));
            throw;
        }
    }
}