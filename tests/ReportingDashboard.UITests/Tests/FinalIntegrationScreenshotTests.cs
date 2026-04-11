using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Screenshot readiness tests for the Final Integration PR (#521).
/// Verifies the dashboard can be captured cleanly at 1920x1080 for PowerPoint decks.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FinalIntegrationScreenshotTests
{
    private readonly PlaywrightFixture _fixture;

    public FinalIntegrationScreenshotTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Screenshot_1920x1080_CapturesCleanly()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Verify viewport
            var viewport = page.ViewportSize;
            Assert.NotNull(viewport);
            Assert.Equal(1920, viewport!.Width);
            Assert.Equal(1080, viewport.Height);

            // Take screenshot and verify it's non-empty
            var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = false,
                Type = ScreenshotType.Png
            });

            Assert.True(screenshot.Length > 1000,
                "Screenshot should be a non-trivial PNG image");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Screenshot_1920x1080_CapturesCleanly));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Screenshot_NoClipping_AllSectionsInView()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Verify all sections fit within viewport bounds
            var headerBox = await dashboard.Header.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineArea.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapWrap.BoundingBoxAsync();

            Assert.NotNull(headerBox);
            Assert.NotNull(tlBox);
            Assert.NotNull(hmBox);

            // All sections should start at x=0 (or near it) and end within 1920px
            Assert.True(headerBox!.X >= 0 && headerBox.X + headerBox.Width <= 1920);
            Assert.True(tlBox!.X >= 0 && tlBox.X + tlBox.Width <= 1920);
            Assert.True(hmBox!.X >= 0 && hmBox.X + hmBox.Width <= 1920);

            // Heatmap bottom should be within 1080px
            var hmBottom = hmBox.Y + hmBox.Height;
            Assert.True(hmBottom <= 1080,
                $"Heatmap bottom ({hmBottom}px) should be within 1080px viewport");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Screenshot_NoClipping_AllSectionsInView));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Screenshot_NoBrowserChrome_InOutput()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Body should have white background
            var bgColor = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).backgroundColor");
            // rgb(255, 255, 255) = #FFFFFF
            Assert.Contains("255", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Screenshot_NoBrowserChrome_InOutput));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ViewportMeta_WidthIs1920()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var viewportContent = await page.EvaluateAsync<string>(
                "() => document.querySelector('meta[name=\"viewport\"]')?.content ?? ''");
            Assert.Contains("1920", viewportContent);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ViewportMeta_WidthIs1920));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_CssStylesheet_IsLoaded()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var hasStylesheet = await page.EvaluateAsync<bool>(
                "() => Array.from(document.styleSheets).some(s => s.href && s.href.includes('dashboard.css'))");
            Assert.True(hasStylesheet, "dashboard.css should be loaded");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_CssStylesheet_IsLoaded));
            throw;
        }
    }
}