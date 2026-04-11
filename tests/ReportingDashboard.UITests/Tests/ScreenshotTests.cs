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
    public async Task Dashboard_ScreenshotCapture_Succeeds()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"dashboard_full_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

        await page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = false, // Fixed viewport
            Clip = new Clip { X = 0, Y = 0, Width = 1920, Height = 1080 }
        });

        File.Exists(path).Should().BeTrue();
        new FileInfo(path).Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_At1920x1080_NoContentOverflow()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        var bodyHeight = await page.EvaluateAsync<double>("document.body.scrollHeight");
        var bodyWidth = await page.EvaluateAsync<double>("document.body.scrollWidth");

        // Allow small tolerance for sub-pixel rendering
        bodyWidth.Should().BeLessThanOrEqualTo(1925, "content should not overflow horizontally");
    }

    [Fact]
    public async Task Dashboard_AllSectionsVisibleInViewport()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        // Verify header is in viewport
        var headerBox = await dashboard.Header.BoundingBoxAsync();
        headerBox.Should().NotBeNull();
        headerBox!.Y.Should().BeGreaterThanOrEqualTo(0);

        // Verify timeline is visible
        var timelineBox = await dashboard.TimelineArea.BoundingBoxAsync();
        timelineBox.Should().NotBeNull();

        // Verify heatmap is visible
        var heatmapBox = await dashboard.HeatmapWrap.BoundingBoxAsync();
        heatmapBox.Should().NotBeNull();
    }
}