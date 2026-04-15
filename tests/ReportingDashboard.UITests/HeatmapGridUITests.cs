using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapGridUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public HeatmapGridUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.CreatePageAsync();
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_DisplaysSectionTitle_MonthlyExecutionHeatmap()
    {
        // The HeatmapGrid renders a .hm-title with "Monthly Execution Heatmap"
        var title = _page.Locator(".hm-title");
        await title.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var text = await title.TextContentAsync();
        text.Should().Be("Monthly Execution Heatmap");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_RendersCornerCell_WithStatusText()
    {
        // The corner cell has class .hm-corner and displays "Status"
        var corner = _page.Locator(".hm-corner");
        await corner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var text = await corner.TextContentAsync();
        text.Should().Be("Status");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_RendersColumnHeaders_WithMonthNames()
    {
        // Column headers use .hm-col-hdr class; there should be at least 3 month columns
        var headers = _page.Locator(".hm-col-hdr");
        await headers.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var count = await headers.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(3, "heatmap should have at least 3 month columns");

        // Each header should have non-empty text (month names)
        for (int i = 0; i < count; i++)
        {
            var headerText = await headers.Nth(i).TextContentAsync();
            headerText.Should().NotBeNullOrWhiteSpace($"column header {i} should display a month name");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_RendersHighlightedColumn_WithGoldBackground()
    {
        // The highlighted column header gets .hm-col-highlight class
        var highlightedHeaders = _page.Locator(".hm-col-hdr.hm-col-highlight");

        // Wait for at least the grid to render
        await _page.Locator(".hm-grid").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var count = await highlightedHeaders.CountAsync();
        // There should be 0 or 1 highlighted column (0 if highlightColumnIndex is out of range)
        count.Should().BeLessThanOrEqualTo(1, "at most one column should be highlighted");

        if (count == 1)
        {
            // Verify the highlighted column has the gold background via CSS class presence
            var classes = await highlightedHeaders.First.GetAttributeAsync("class");
            classes.Should().Contain("hm-col-highlight");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_RendersFourStatusRows_WithCategoryHeaders()
    {
        // Four row headers exist with .hm-row-hdr class
        var rowHeaders = _page.Locator(".hm-row-hdr");
        await rowHeaders.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var count = await rowHeaders.CountAsync();
        count.Should().Be(4, "heatmap should have exactly 4 status rows (Shipped, In Progress, Carryover, Blockers)");

        // Collect all row header texts
        var rowTexts = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var text = await rowHeaders.Nth(i).TextContentAsync();
            rowTexts.Add(text?.Trim() ?? "");
        }

        // Each row header should have non-empty text
        rowTexts.Should().AllSatisfy(t => t.Should().NotBeNullOrWhiteSpace());
    }
}

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapCellUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public HeatmapCellUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.CreatePageAsync();
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapCells_RenderWithThemedClasses()
    {
        // Cells should have theme-specific CSS classes from the source: ship-cell, prog-cell, carry-cell, block-cell
        await _page.Locator(".hm-grid").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var shipCells = await _page.Locator(".hm-cell.ship-cell").CountAsync();
        var progCells = await _page.Locator(".hm-cell.prog-cell").CountAsync();
        var carryCells = await _page.Locator(".hm-cell.carry-cell").CountAsync();
        var blockCells = await _page.Locator(".hm-cell.block-cell").CountAsync();

        // Each row should have cells matching the column count
        shipCells.Should().BeGreaterThan(0, "shipped row should have themed cells");
        progCells.Should().BeGreaterThan(0, "progress row should have themed cells");
        carryCells.Should().BeGreaterThan(0, "carryover row should have themed cells");
        blockCells.Should().BeGreaterThan(0, "blockers row should have themed cells");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapCells_DisplayItemsOrEmptyDash()
    {
        // Each .hm-cell contains .it divs - either with content or with "-" for empty
        await _page.Locator(".hm-grid").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var allItems = _page.Locator(".hm-cell .it");
        var itemCount = await allItems.CountAsync();
        itemCount.Should().BeGreaterThan(0, "cells should contain items or empty dash markers");

        // Check that empty cells show "-"
        var emptyItems = _page.Locator(".hm-cell .it.empty");
        var emptyCount = await emptyItems.CountAsync();

        if (emptyCount > 0)
        {
            var emptyText = await emptyItems.First.TextContentAsync();
            emptyText.Should().Be("-", "empty cells should display a dash");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapCells_HighlightedCellsHaveHighlightClass()
    {
        // Highlighted cells get the "highlight" CSS class
        await _page.Locator(".hm-grid").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var highlightedCells = _page.Locator(".hm-cell.highlight");
        var highlightCount = await highlightedCells.CountAsync();

        // If there's a valid highlight column, each of the 4 rows should have one highlighted cell
        var highlightedHeader = await _page.Locator(".hm-col-hdr.hm-col-highlight").CountAsync();
        if (highlightedHeader == 1)
        {
            highlightCount.Should().Be(4, "each of the 4 rows should have one highlighted cell");
        }
        else
        {
            highlightCount.Should().Be(0, "no cells should be highlighted when no column is highlighted");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_CssGridLayout_HasCorrectStructure()
    {
        // Verify the grid container has the expected inline style for grid-template-columns
        var grid = _page.Locator(".hm-grid");
        await grid.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var style = await grid.GetAttributeAsync("style");
        style.Should().NotBeNull();
        style.Should().Contain("grid-template-columns");
        style.Should().Contain("160px");
        style.Should().Contain("repeat(");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapRows_UseDisplayContents_ForGridParticipation()
    {
        // HeatmapRow wraps children in a div with style="display:contents"
        await _page.Locator(".hm-grid").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var contentsWrappers = _page.Locator(".hm-grid > div[style*='display:contents']");
        var count = await contentsWrappers.CountAsync();
        count.Should().Be(4, "each of the 4 HeatmapRow components should render a display:contents wrapper");
    }
}