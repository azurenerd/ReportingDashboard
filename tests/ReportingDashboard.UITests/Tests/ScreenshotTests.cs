using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ScreenshotTests
{
    private readonly PlaywrightFixture _fixture;

    public ScreenshotTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Dashboard_CanCaptureScreenshot()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        // Verify we can take a screenshot without error
        var screenshotsDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotsDir);
        var path = Path.Combine(screenshotsDir, $"dashboard_test_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = false, // 1920x1080 viewport only
            Type = ScreenshotType.Png
        });

        File.Exists(path).Should().BeTrue();
        new FileInfo(path).Length.Should().BeGreaterThan(0);

        // Clean up
        File.Delete(path);
    }

    [Fact]
    public async Task Dashboard_ScreenshotFixtureHelper_Works()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        await dashboardPage.NavigateAsync();

        // Use the fixture helper
        await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ScreenshotFixtureHelper_Works));

        var screenshotsDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        var files = Directory.GetFiles(screenshotsDir, $"{nameof(Dashboard_ScreenshotFixtureHelper_Works)}*.png");
        files.Should().NotBeEmpty();

        // Clean up
        foreach (var file in files)
            File.Delete(file);
    }
}