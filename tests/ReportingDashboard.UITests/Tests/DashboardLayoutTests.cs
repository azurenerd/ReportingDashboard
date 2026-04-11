using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardLayoutTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardLayoutTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_LoadsWithoutErrors()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.True(await dashboard.IsDashboardVisibleAsync(),
                "Expected .dashboard-root to be present");
            Assert.False(await dashboard.IsErrorVisibleAsync(),
                "Expected no .error-panel");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_LoadsWithoutErrors));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HasThreeMajorSections()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasThreeMajorSections));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ViewportIs1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var viewport = page.ViewportSize;
            Assert.NotNull(viewport);
            Assert.Equal(1920, viewport!.Width);
            Assert.Equal(1080, viewport.Height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ViewportIs1920x1080));
            throw;
        }
    }
}