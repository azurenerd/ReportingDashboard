using Microsoft.Playwright;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_LoadsWithHeader()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        await Expect(header).ToBeVisibleAsync();

        var title = page.Locator(".hdr h1");
        await Expect(title).ToBeVisibleAsync();
        var titleText = await title.TextContentAsync();
        Assert.NotNull(titleText);
        Assert.NotEmpty(titleText!);
    }

    [Fact]
    public async Task Dashboard_RendersTimelineSection()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timeline = page.Locator(".tl-area");
        await Expect(timeline).ToBeVisibleAsync();

        var svg = page.Locator(".tl-svg-box svg");
        await Expect(svg).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Dashboard_RendersHeatmapGrid()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmap = page.Locator(".hm-wrap");
        await Expect(heatmap).ToBeVisibleAsync();

        var title = page.Locator(".hm-title");
        var titleText = await title.TextContentAsync();
        Assert.Contains("Monthly Execution Heatmap", titleText!);
    }

    [Fact]
    public async Task Dashboard_LegendShowsFourItems()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendItems = page.Locator(".legend .legend-item");
        await Expect(legendItems).ToHaveCountAsync(4);

        await Expect(page.GetByText("PoC Milestone")).ToBeVisibleAsync();
        await Expect(page.GetByText("Production Release")).ToBeVisibleAsync();
        await Expect(page.GetByText("Checkpoint")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Dashboard_NoScrollbarsAt1920x1080()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasScrollbar = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollHeight > document.documentElement.clientHeight || document.documentElement.scrollWidth > document.documentElement.clientWidth");

        Assert.False(hasScrollbar, "Page should not have scrollbars at 1920x1080");
    }

    private static ILocatorAssertions Expect(ILocator locator)
        => Assertions.Expect(locator);
}