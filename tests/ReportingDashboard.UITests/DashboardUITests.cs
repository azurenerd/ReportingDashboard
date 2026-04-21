using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

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
    public async Task Dashboard_LoadsAtRootUrl_DisplaysHeader()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        (await header.IsVisibleAsync()).Should().BeTrue();

        var h1 = page.Locator("h1");
        var titleText = await h1.TextContentAsync();
        titleText.Should().NotBeNullOrEmpty();

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_PageFitsViewport_NoScrollbars()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollHeight = await page.EvaluateAsync<int>("document.documentElement.scrollHeight");
        var clientHeight = await page.EvaluateAsync<int>("document.documentElement.clientHeight");

        scrollHeight.Should().BeLessOrEqualTo(clientHeight + 5,
            "page should fit within 1080px viewport without vertical scrollbar");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_TimelineSection_IsVisible()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timeline = page.Locator(".tl-area");
        (await timeline.IsVisibleAsync()).Should().BeTrue();

        var svg = page.Locator(".tl-svg-box svg");
        (await svg.IsVisibleAsync()).Should().BeTrue();

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_HeatmapGrid_RendersWithStatusRows()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmap = page.Locator(".hm-wrap");
        (await heatmap.IsVisibleAsync()).Should().BeTrue();

        var rowHeaders = page.Locator(".hm-row-hdr");
        var count = await rowHeaders.CountAsync();
        count.Should().BeGreaterOrEqualTo(1, "should have at least one status row");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_Legend_DisplaysFourItems()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legend = page.Locator(".legend");
        (await legend.IsVisibleAsync()).Should().BeTrue();

        var legendItems = page.Locator(".legend-item");
        var count = await legendItems.CountAsync();
        count.Should().Be(4, "legend should have PoC, Production, Checkpoint, and Now items");

        await page.CloseAsync();
    }
}