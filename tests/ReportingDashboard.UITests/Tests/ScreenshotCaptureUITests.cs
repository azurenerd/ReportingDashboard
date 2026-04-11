using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

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
    public async Task Dashboard_FullPage_CanBeCapturedAt1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
        Directory.CreateDirectory(screenshotDir);
        var path = Path.Combine(screenshotDir, $"full_dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        var bytes = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            Clip = new Clip { X = 0, Y = 0, Width = 1920, Height = 1080 }
        });

        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0, "screenshot should have non-zero byte length");
        File.Exists(path).Should().BeTrue("screenshot file should be saved to disk");
    }

    [Fact]
    public async Task Dashboard_ContentFits_Within1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        // Check that the body doesn't exceed the viewport
        var bodyWidth = await page.EvalOnSelectorAsync<double>("body",
            "el => el.scrollWidth");
        var bodyHeight = await page.EvalOnSelectorAsync<double>("body",
            "el => el.scrollHeight");

        bodyWidth.Should().BeLessThanOrEqualTo(1920, "body scroll width should not exceed 1920px");
        bodyHeight.Should().BeLessThanOrEqualTo(1080, "body scroll height should not exceed 1080px");
    }
}