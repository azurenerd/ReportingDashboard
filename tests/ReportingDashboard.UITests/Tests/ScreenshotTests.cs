using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests focused on screenshot-readiness of the dashboard at 1920x1080.
/// Validates that the dashboard renders without scrollbars and fits the viewport.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ScreenshotTests
{
    private readonly PlaywrightFixture _fixture;

    public ScreenshotTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_FitsWithin1920x1080_NoHorizontalScrollbar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var hasHScroll = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
            Assert.False(hasHScroll, "Dashboard should not have a horizontal scrollbar at 1920px width");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_FitsWithin1920x1080_NoHorizontalScrollbar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_CanCaptureScreenshot()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.DashboardRoot).ToBeVisibleAsync();

            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotDir);
            var path = Path.Combine(screenshotDir, $"dashboard_capture_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = path,
                Clip = new Clip { X = 0, Y = 0, Width = 1920, Height = 1080 }
            });

            Assert.True(File.Exists(path), "Screenshot file should have been created");
            var fileInfo = new FileInfo(path);
            Assert.True(fileInfo.Length > 1000, "Screenshot should be a non-trivial image file");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_CanCaptureScreenshot));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_AllSectionsRenderWithinViewport()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Check that the heatmap bottom is within the viewport
            var heatmapBottom = await dashboard.HeatmapWrap.EvaluateAsync<double>(
                "el => { const r = el.getBoundingClientRect(); return r.bottom; }");
            Assert.True(heatmapBottom <= 1080,
                $"Heatmap bottom ({heatmapBottom}px) should be within 1080px viewport");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_AllSectionsRenderWithinViewport));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_StaticFilesLoad_CssApplied()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Verify CSS is loaded by checking that .hdr has flex display
            var display = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_StaticFilesLoad_CssApplied));
            throw;
        }
    }
}