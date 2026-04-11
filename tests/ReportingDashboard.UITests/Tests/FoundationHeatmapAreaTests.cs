using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the Heatmap wrapper area styling from PR #539.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationHeatmapAreaTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationHeatmapAreaTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapWrap_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapWrap_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapWrap_HasFlexColumnDisplay()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var display = await dashboard.HeatmapWrap.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            var direction = await dashboard.HeatmapWrap.EvaluateAsync<string>(
                "el => getComputedStyle(el).flexDirection");
            Assert.Equal("flex", display);
            Assert.Equal("column", direction);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapWrap_HasFlexColumnDisplay));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapWrap_FlexGrowsToFillSpace()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var flex = await dashboard.HeatmapWrap.EvaluateAsync<string>(
                "el => getComputedStyle(el).flexGrow");
            Assert.Equal("1", flex);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapWrap_FlexGrowsToFillSpace));
            throw;
        }
    }
}