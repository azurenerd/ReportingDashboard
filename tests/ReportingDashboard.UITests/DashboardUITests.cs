using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

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
    public async Task Dashboard_LoadsWithinViewport_NoScrollbars()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyWidth = await page.EvalOnSelectorAsync<int>("body", "el => el.scrollWidth");
        var bodyHeight = await page.EvalOnSelectorAsync<int>("body", "el => el.scrollHeight");

        bodyWidth.Should().BeLessThanOrEqualTo(1920);
        bodyHeight.Should().BeLessThanOrEqualTo(1080);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_DisplaysHeaderWithTitleAndLegend()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        await header.WaitForAsync();
        (await header.IsVisibleAsync()).Should().BeTrue();

        var h1 = page.Locator("h1");
        (await h1.IsVisibleAsync()).Should().BeTrue();
        var titleText = await h1.TextContentAsync();
        titleText.Should().NotBeNullOrWhiteSpace();

        var adoLink = page.Locator("h1 a");
        (await adoLink.IsVisibleAsync()).Should().BeTrue();
        var linkText = await adoLink.TextContentAsync();
        linkText.Should().Contain("ADO Backlog");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_RendersTimelineWithSvg()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = page.Locator(".tl-area");
        await tlArea.WaitForAsync();
        (await tlArea.IsVisibleAsync()).Should().BeTrue();

        var svg = page.Locator(".tl-svg-box svg");
        (await svg.CountAsync()).Should().BeGreaterThan(0);

        // Verify NOW line exists (red dashed line with stroke #EA4335)
        var nowLine = page.Locator("svg line[stroke='#EA4335']");
        (await nowLine.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_RendersHeatmapGrid()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hmWrap = page.Locator(".hm-wrap");
        await hmWrap.WaitForAsync();
        (await hmWrap.IsVisibleAsync()).Should().BeTrue();

        var hmTitle = page.Locator(".hm-title");
        var titleText = await hmTitle.TextContentAsync();
        titleText.Should().Contain("Heatmap");

        var colHeaders = page.Locator(".hm-col-hdr");
        (await colHeaders.CountAsync()).Should().BeGreaterThan(0);

        var rowHeaders = page.Locator(".hm-row-hdr");
        (await rowHeaders.CountAsync()).Should().Be(4);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_HeatmapHasColorCodedRows()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var shipHdr = page.Locator(".ship-hdr");
        (await shipHdr.CountAsync()).Should().BeGreaterThan(0);

        var progHdr = page.Locator(".prog-hdr");
        (await progHdr.CountAsync()).Should().BeGreaterThan(0);

        var carryHdr = page.Locator(".carry-hdr");
        (await carryHdr.CountAsync()).Should().BeGreaterThan(0);

        var blockHdr = page.Locator(".block-hdr");
        (await blockHdr.CountAsync()).Should().BeGreaterThan(0);
    }
}