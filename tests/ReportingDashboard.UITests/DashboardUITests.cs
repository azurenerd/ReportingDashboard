using FluentAssertions;
using Microsoft.Playwright;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_LoadsWithHeaderAndTitle()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        await header.WaitForAsync();
        (await header.IsVisibleAsync()).Should().BeTrue();

        var h1 = page.Locator("h1");
        var titleText = await h1.TextContentAsync();
        titleText.Should().NotBeNullOrEmpty();
        titleText.Should().Contain("ADO Backlog");

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_RendersTimelineSection()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timeline = page.Locator(".tl-area");
        await timeline.WaitForAsync();
        (await timeline.IsVisibleAsync()).Should().BeTrue();

        var svg = page.Locator("svg");
        (await svg.IsVisibleAsync()).Should().BeTrue();

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_RendersHeatmapGrid()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmap = page.Locator(".hm-wrap");
        await heatmap.WaitForAsync();
        (await heatmap.IsVisibleAsync()).Should().BeTrue();

        var title = page.Locator(".hm-title");
        var titleText = await title.TextContentAsync();
        titleText.Should().Contain("Heatmap");

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_HeaderLegendHasFourItems()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendItems = page.Locator(".legend .legend-item");
        var count = await legendItems.CountAsync();
        count.Should().Be(4);

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_FitsViewportWithNoScrollbars()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollHeight = await page.EvaluateAsync<int>("document.documentElement.scrollHeight");
        var clientHeight = await page.EvaluateAsync<int>("document.documentElement.clientHeight");

        scrollHeight.Should().BeLessThanOrEqualTo(clientHeight + 5,
            "page should fit within 1080px viewport without vertical scrollbars");

        await page.CloseAsync();
    }
}