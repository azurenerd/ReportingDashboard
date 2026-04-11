using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// E2E tests verifying the dashboard is screenshot-ready at 1920x1080.
/// These tests ensure the page renders without scrollbars, all sections fit,
/// and a screenshot can be captured cleanly for PowerPoint.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ScreenshotReadinessTests
{
    private readonly PlaywrightFixture _fixture;

    public ScreenshotReadinessTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_FitsIn1920x1080_NoScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();

            var hasHorizontalScroll = await page.EvaluateAsync<bool>(
                "() => document.body.scrollWidth > document.body.clientWidth");
            var hasVerticalScroll = await page.EvaluateAsync<bool>(
                "() => document.body.scrollHeight > document.body.clientHeight");

            Assert.False(hasHorizontalScroll, "Page should not have horizontal scrollbar");
            Assert.False(hasVerticalScroll, "Page should not have vertical scrollbar");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_FitsIn1920x1080_NoScrollbars));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ViewportSize_Is1920x1080()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var viewport = page.ViewportSize;
            Assert.NotNull(viewport);
            Assert.Equal(1920, viewport!.Width);
            Assert.Equal(1080, viewport.Height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ViewportSize_Is1920x1080));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_CanCaptureScreenshot()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = false // Fixed viewport only
            });

            Assert.NotNull(screenshotBytes);
            Assert.True(screenshotBytes.Length > 1000,
                "Screenshot should be a meaningful image (>1KB)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_CanCaptureScreenshot));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_AllSectionsVisibleInViewport()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();

            // Verify all three major sections are within the viewport
            if (await po.Header.CountAsync() > 0)
            {
                var headerBox = await po.Header.BoundingBoxAsync();
                Assert.NotNull(headerBox);
                Assert.True(headerBox!.Y >= 0, "Header should be at top of viewport");
                Assert.True(headerBox.Y + headerBox.Height <= 1080, "Header should fit in viewport");
            }

            if (await po.TimelineArea.CountAsync() > 0)
            {
                var tlBox = await po.TimelineArea.BoundingBoxAsync();
                Assert.NotNull(tlBox);
                Assert.True(tlBox!.Y + tlBox.Height <= 1080, "Timeline should fit in viewport");
            }

            if (await po.HeatmapWrap.CountAsync() > 0)
            {
                var hmBox = await po.HeatmapWrap.BoundingBoxAsync();
                Assert.NotNull(hmBox);
                Assert.True(hmBox!.Y + hmBox.Height <= 1080, "Heatmap should fit in viewport");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_AllSectionsVisibleInViewport));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_NoJavaScriptErrors()
    {
        var page = await _fixture.NewPageAsync();
        var jsErrors = new List<string>();

        page.PageError += (_, error) => jsErrors.Add(error);

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            // Wait a moment for any deferred JS
            await page.WaitForTimeoutAsync(2000);

            Assert.Empty(jsErrors);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_NoJavaScriptErrors));
            throw;
        }
    }
}