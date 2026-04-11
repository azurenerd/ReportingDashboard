using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection(PlaywrightCollection.Name)]
public class ScreenshotTests
{
    private readonly PlaywrightFixture _fixture;

    public ScreenshotTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_CanCaptureFullPageScreenshot()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();

        var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true,
            Type = ScreenshotType.Png
        });

        screenshot.Should().NotBeNullOrEmpty();
        screenshot.Length.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_ScreenshotSavesToFile()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();

        var tempPath = Path.Combine(Path.GetTempPath(), $"dashboard_screenshot_{Guid.NewGuid():N}.png");
        try
        {
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = tempPath,
                FullPage = true
            });

            File.Exists(tempPath).Should().BeTrue();
            new FileInfo(tempPath).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}