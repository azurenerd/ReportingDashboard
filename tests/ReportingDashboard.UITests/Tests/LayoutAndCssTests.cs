using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class LayoutAndCssTests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutAndCssTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_FitsWithin1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var hasHScroll = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasHScroll, "No horizontal scrollbar expected at 1920px");
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasFlexDisplay()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var display = await dashboard.Header.EvaluateAsync<string>(
            "el => getComputedStyle(el).display");
        Assert.Equal("flex", display);
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var borderStyle = await dashboard.Header.EvaluateAsync<string>(
            "el => getComputedStyle(el).borderBottomStyle");
        Assert.Equal("solid", borderStyle);
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_GridUsesCorrectColumnTemplate()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var style = await dashboard.HeatmapGrid.GetAttributeAsync("style") ?? "";
        Assert.Contains("160px", style);
        Assert.Contains("1fr", style);
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasFafafaBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var bg = await dashboard.TimelineArea.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        Assert.Contains("250", bg);
    }
}