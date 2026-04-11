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
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        // Verify screenshot capability works (key use case: PowerPoint capture)
        await _fixture.CaptureScreenshotAsync(page, "dashboard_test");

        var screenshotDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        var files = Directory.GetFiles(screenshotDir, "dashboard_test_*.png");
        files.Should().NotBeEmpty("screenshot should have been saved");
    }

    [Fact]
    public async Task Dashboard_ScreenshotDimensions_Are1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        // The viewport is set to 1920x1080 in the fixture
        var viewport = page.ViewportSize;
        viewport.Should().NotBeNull();
        viewport!.Width.Should().Be(1920);
        viewport.Height.Should().Be(1080);
    }

    [Fact]
    public async Task Dashboard_NoScrollbarsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var count = await container.CountAsync();
        if (count == 0) return;

        var overflow = await container.EvaluateAsync<string>(
            "el => getComputedStyle(el).overflow");
        overflow.Should().Be("hidden", "main container should hide overflow for clean screenshots");
    }

    [Fact]
    public async Task Dashboard_ContainerFitsExactlyInViewport()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var container = dp.MainContainer;
        var count = await container.CountAsync();
        if (count == 0) return;

        var box = await container.BoundingBoxAsync();
        box.Should().NotBeNull();
        box!.Width.Should().BeApproximately(1920, 2);
        box.Height.Should().BeApproximately(1080, 2);
    }
}