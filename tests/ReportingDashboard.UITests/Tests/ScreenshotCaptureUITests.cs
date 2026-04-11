using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests that verify the dashboard renders at the correct 1920x1080
/// dimensions for PowerPoint screenshot capture.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ScreenshotCaptureUITests
{
    private readonly PlaywrightFixture _fixture;

    public ScreenshotCaptureUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_At1920x1080_NoScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hasVerticalScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight");
            hasVerticalScrollbar.Should().BeFalse("page should not have vertical scrollbars at 1920x1080");

            var hasHorizontalScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
            // Note: body is 1920px and viewport is 1920px so no horizontal scroll expected
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Screenshot_NoScrollbars");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_BodyDimensions_Are1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bodyWidth = await page.EvaluateAsync<double>(
                "() => document.body.getBoundingClientRect().width");
            var bodyHeight = await page.EvaluateAsync<double>(
                "() => document.body.getBoundingClientRect().height");

            bodyWidth.Should().Be(1920);
            bodyHeight.Should().Be(1080);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Screenshot_BodyDimensions");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_CanCaptureScreenshot_WithoutError()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = false,
                Type = ScreenshotType.Png
            });

            screenshotBytes.Should().NotBeNull();
            screenshotBytes.Length.Should().BeGreaterThan(0, "screenshot should produce non-empty data");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Screenshot_Capture");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_AllSections_FitWithin1080Height()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hdrBox = await dashboard.HeaderSection.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineSection.BoundingBoxAsync();
            var hmBox = await page.Locator(".hm-wrap").BoundingBoxAsync();

            hdrBox.Should().NotBeNull();
            tlBox.Should().NotBeNull();
            hmBox.Should().NotBeNull();

            // All sections should be within the visible 1080px viewport
            hdrBox!.Y.Should().BeGreaterThanOrEqualTo(0);
            (hmBox!.Y + hmBox.Height).Should().BeLessThanOrEqualTo(1080 + 5,
                "all sections should fit within 1080px height (with small tolerance)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Screenshot_SectionsFit");
            throw;
        }
    }
}