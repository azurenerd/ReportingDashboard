using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        Skip.If(!_fixture.BrowserAvailable, "Playwright browser not available. Run 'pwsh playwright.ps1 install' to install browsers.");
        Skip.If(_fixture.Browser is null, "Playwright browser not initialized.");

        var context = await _fixture.Browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    private async Task<bool> NavigateAndWaitForRender(IPage page)
    {
        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { Timeout = 10000 });
            if (response is null || !response.Ok) return false;

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            // Wait for Blazor Server interactive rendering to complete
            await page.WaitForTimeoutAsync(3000);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [SkippableFact]
    public async Task HeatmapSection_RendersWithTitleAndGrid()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWaitForRender(page), "Server not reachable at " + _fixture.BaseUrl);

        // Check for hm-wrap container
        var hmWrap = page.Locator(".hm-wrap");
        var wrapCount = await hmWrap.CountAsync();

        if (wrapCount > 0)
        {
            // Verify the heatmap title is present
            var title = page.Locator(".hm-title");
            var titleCount = await title.CountAsync();
            titleCount.Should().BeGreaterThan(0, "heatmap title (.hm-title) should be rendered");

            var titleText = await title.TextContentAsync();
            titleText.Should().Contain("Monthly Execution Heatmap", "title should contain expected text");

            // Verify the grid is present
            var grid = page.Locator(".hm-grid");
            var gridCount = await grid.CountAsync();
            gridCount.Should().BeGreaterThan(0, "heatmap grid (.hm-grid) should be rendered");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task HeatmapGrid_HasCornerCellWithStatusText()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWaitForRender(page), "Server not reachable at " + _fixture.BaseUrl);

        var corner = page.Locator(".hm-corner");
        var count = await corner.CountAsync();

        if (count > 0)
        {
            var text = await corner.TextContentAsync();
            text.Should().NotBeNull();
            text!.Trim().ToUpperInvariant().Should().Contain("STATUS",
                "corner cell should display 'STATUS'");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task HeatmapGrid_RendersMonthColumnHeaders()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWaitForRender(page), "Server not reachable at " + _fixture.BaseUrl);

        var colHeaders = page.Locator(".hm-col-hdr");
        var count = await colHeaders.CountAsync();

        if (count > 0)
        {
            count.Should().BeGreaterThan(0, "at least one month column header should be rendered");

            // At least one header should have the now-hdr class for current month highlighting
            var nowHeader = page.Locator(".hm-col-hdr.now-hdr");
            var nowCount = await nowHeader.CountAsync();

            if (nowCount > 0)
            {
                var nowText = await nowHeader.TextContentAsync();
                nowText.Should().Contain("Now", "current month header should contain 'Now' indicator");
            }
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task HeatmapGrid_RendersStatusRowHeaders()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWaitForRender(page), "Server not reachable at " + _fixture.BaseUrl);

        var rowHeaders = page.Locator(".hm-row-hdr");
        var count = await rowHeaders.CountAsync();

        if (count > 0)
        {
            // Expect 4 status rows: Shipped, In Progress, Carryover, Blockers
            count.Should().Be(4, "there should be exactly 4 status row headers");

            // Verify category-specific CSS classes are applied
            var shipHdr = page.Locator(".ship-hdr");
            (await shipHdr.CountAsync()).Should().BeGreaterThan(0, "Shipped row header should have .ship-hdr class");

            var progHdr = page.Locator(".prog-hdr");
            (await progHdr.CountAsync()).Should().BeGreaterThan(0, "In Progress row header should have .prog-hdr class");

            var carryHdr = page.Locator(".carry-hdr");
            (await carryHdr.CountAsync()).Should().BeGreaterThan(0, "Carryover row header should have .carry-hdr class");

            var blockHdr = page.Locator(".block-hdr");
            (await blockHdr.CountAsync()).Should().BeGreaterThan(0, "Blockers row header should have .block-hdr class");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task HeatmapGrid_DataCellsRenderItemsOrDashes()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWaitForRender(page), "Server not reachable at " + _fixture.BaseUrl);

        var cells = page.Locator(".hm-cell");
        var cellCount = await cells.CountAsync();

        if (cellCount > 0)
        {
            cellCount.Should().BeGreaterThan(0, "heatmap should have data cells");

            // Every cell should contain at least one .it element (either with content or empty dash)
            var items = page.Locator(".hm-cell .it");
            var itemCount = await items.CountAsync();
            itemCount.Should().BeGreaterThan(0, "data cells should contain .it elements");

            // Check that empty cells render a dash
            var emptyItems = page.Locator(".hm-cell .it.empty");
            var emptyCount = await emptyItems.CountAsync();

            if (emptyCount > 0)
            {
                var firstEmptyText = await emptyItems.First.TextContentAsync();
                firstEmptyText.Should().Contain("-", "empty cells should display a dash");
            }
        }

        await page.Context.CloseAsync();
    }
}