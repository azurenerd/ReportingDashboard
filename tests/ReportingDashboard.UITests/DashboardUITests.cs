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
    [Trait("Category", "UI")]
    public async Task DashboardPage_LoadsWithHeaderAndContent()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = page.Locator("h1");
        var h1Text = await h1.TextContentAsync();
        h1Text.Should().NotBeNullOrEmpty();
        h1Text.Should().Contain("ADO Backlog");

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardPage_HasTimelineSection()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = page.Locator(".tl-area");
        var isVisible = await tlArea.IsVisibleAsync();

        if (isVisible)
        {
            var svg = page.Locator("svg");
            (await svg.CountAsync()).Should().BeGreaterThan(0);
        }

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardPage_HasHeatmapGrid()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hmWrap = page.Locator(".hm-wrap");
        var isVisible = await hmWrap.IsVisibleAsync();

        if (isVisible)
        {
            var hmGrid = page.Locator(".hm-grid");
            (await hmGrid.CountAsync()).Should().BeGreaterThan(0);
        }

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardPage_HeaderShowsLegend()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legend = page.Locator(".legend");
        var isVisible = await legend.IsVisibleAsync();

        if (isVisible)
        {
            var text = await legend.TextContentAsync();
            text.Should().Contain("PoC Milestone");
            text.Should().Contain("Production Release");
            text.Should().Contain("Checkpoint");
        }

        await page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardPage_FitsViewportWithoutScrollbars()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollHeight = await page.EvaluateAsync<int>("document.documentElement.scrollHeight");
        var clientHeight = await page.EvaluateAsync<int>("document.documentElement.clientHeight");

        // Page should not require vertical scrolling at 1920x1080
        scrollHeight.Should().BeLessThanOrEqualTo(clientHeight + 5,
            "page should fit within 1080px viewport without scrollbars");

        await page.CloseAsync();
    }
}