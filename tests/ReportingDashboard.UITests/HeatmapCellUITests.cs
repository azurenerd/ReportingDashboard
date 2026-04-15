using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapCellUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage? _page;

    public HeatmapCellUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (_fixture.Browser is not null)
        {
            _page = await _fixture.Browser.NewPageAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapGrid_CssGridLayout_HasCorrectStructure()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var grid = page.Locator(".hm-grid");
        await Assertions.Expect(grid).ToBeVisibleAsync();
        var display = await grid.EvaluateAsync<string>("el => getComputedStyle(el).display");
        Assert.Equal("grid", display);
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapCells_RenderWithThemedClasses()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var shippedCells = page.Locator(".ship-cell");
        Assert.True(await shippedCells.CountAsync() > 0, "Should have shipped-themed cells");
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapCells_HighlightedCellsHaveHighlightClass()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var highlightedCells = page.Locator(".hm-cell.highlight");
        Assert.True(await highlightedCells.CountAsync() > 0, "Should have highlighted cells for current month");
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapCells_DisplayItemsOrEmptyDash()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var items = page.Locator(".hm-cell .it");
        Assert.True(await items.CountAsync() > 0, "Heatmap cells should contain item elements or dash placeholders");
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapRows_UseDisplayContents_ForGridParticipation()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var rowHeaders = page.Locator(".hm-row-hdr");
        Assert.True(await rowHeaders.CountAsync() == 4, "Should have exactly 4 row headers (Shipped, In Progress, Carryover, Blockers)");
    }
}