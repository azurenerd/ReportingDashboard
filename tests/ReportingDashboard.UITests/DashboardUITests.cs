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
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return page;
    }

    [Fact]
    public async Task DashboardPage_LoadsWithHeaderAndTitle()
    {
        var page = await CreatePageAsync();

        var heading = page.Locator("h1");
        await heading.WaitForAsync();
        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrEmpty();
        text.Should().Contain("ADO Backlog");

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_RendersLegendWithFourItems()
    {
        var page = await CreatePageAsync();

        var legendItems = page.Locator(".legend-item");
        var count = await legendItems.CountAsync();
        count.Should().Be(4);

        (await page.GetByText("PoC Milestone").CountAsync()).Should().BeGreaterThan(0);
        (await page.GetByText("Production Release").CountAsync()).Should().BeGreaterThan(0);
        (await page.GetByText("Checkpoint").CountAsync()).Should().BeGreaterThan(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_RendersTimelineSvg()
    {
        var page = await CreatePageAsync();

        var svg = page.Locator("svg");
        await svg.First.WaitForAsync();
        var svgCount = await svg.CountAsync();
        svgCount.Should().BeGreaterThanOrEqualTo(1);

        // Verify NOW text exists in timeline
        var nowText = page.Locator("text:has-text('NOW')");
        (await nowText.CountAsync()).Should().BeGreaterThan(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_RendersHeatmapGrid()
    {
        var page = await CreatePageAsync();

        var heatmapTitle = page.Locator(".hm-title");
        await heatmapTitle.WaitForAsync();
        var titleText = await heatmapTitle.TextContentAsync();
        titleText.Should().Contain("Monthly Execution Heatmap");

        var colHeaders = page.Locator(".hm-col-hdr");
        (await colHeaders.CountAsync()).Should().BeGreaterThan(0);

        var rowHeaders = page.Locator(".hm-row-hdr");
        (await rowHeaders.CountAsync()).Should().Be(4);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_FitsWithinViewportWithNoScrollbars()
    {
        var page = await CreatePageAsync();

        var hasScrollbar = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollHeight > document.documentElement.clientHeight || document.documentElement.scrollWidth > document.documentElement.clientWidth");

        hasScrollbar.Should().BeFalse("page should fit within 1920x1080 with no scrollbars");

        await page.CloseAsync();
    }
}