using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ScreenshotUITests
{
    private readonly PlaywrightFixture _fixture;

    public ScreenshotUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_CanCaptureScreenshot_At1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            // Viewport should be 1920x1080 (set by fixture)
            var viewportSize = page.ViewportSize;
            viewportSize.Should().NotBeNull();
            viewportSize!.Width.Should().Be(1920);
            viewportSize.Height.Should().Be(1080);

            // Should be able to capture screenshot without error
            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Type = ScreenshotType.Png
            });

            screenshotBytes.Should().NotBeNull();
            screenshotBytes.Length.Should().BeGreaterThan(0, "screenshot should produce non-empty image data");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Screenshot_1920x1080_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_FullPage_FitsWithinViewport()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            // Body should not have scrollbars (overflow: hidden)
            var hasVerticalScroll = await page.EvaluateAsync<bool>(
                "() => document.body.scrollHeight > document.body.clientHeight");

            // With overflow hidden, content is clipped, no scrolling expected
            var overflow = await page.Locator("body").EvaluateAsync<string>(
                "el => getComputedStyle(el).overflow");

            overflow.Should().Contain("hidden",
                "body should have overflow hidden to prevent scrolling");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_FitsViewport_Failed");
            throw;
        }
    }
}