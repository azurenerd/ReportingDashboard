using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests validating the dashboard is screenshot-ready at 1920x1080 pixels
/// for PowerPoint capture as specified in PR #539.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationScreenshotTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationScreenshotTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Screenshot_CanBeCapturedAt1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var screenshotPath = Path.Combine(
                _fixture.ScreenshotDir,
                $"dashboard_capture_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = false,
                Clip = new Clip
                {
                    X = 0,
                    Y = 0,
                    Width = 1920,
                    Height = 1080
                }
            });

            Assert.True(File.Exists(screenshotPath), "Screenshot file should exist");
            var fileInfo = new FileInfo(screenshotPath);
            Assert.True(fileInfo.Length > 0, "Screenshot file should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Screenshot_CanBeCapturedAt1920x1080));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasNoHorizontalScrollbar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var hasHorizontalScroll = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
            Assert.False(hasHorizontalScroll, "Page should not have horizontal scrollbar at 1920px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasNoHorizontalScrollbar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasNoVerticalScrollbar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var hasVerticalScroll = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight");
            Assert.False(hasVerticalScroll, "Page should not have vertical scrollbar at 1080px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasNoVerticalScrollbar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task AllContent_FitsWithin1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var bodyWidth = await page.EvaluateAsync<double>(
                "() => document.body.getBoundingClientRect().width");
            var bodyHeight = await page.EvaluateAsync<double>(
                "() => document.body.getBoundingClientRect().height");

            Assert.True(bodyWidth <= 1920, $"Body width {bodyWidth} exceeds 1920px");
            Assert.True(bodyHeight <= 1080, $"Body height {bodyHeight} exceeds 1080px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(AllContent_FitsWithin1920x1080));
            throw;
        }
    }
}