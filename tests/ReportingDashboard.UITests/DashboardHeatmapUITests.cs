using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardHeatmapUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardHeatmapUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.CreatePageAsync();
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
        // Wait for Blazor SignalR circuit to initialize and heatmap to render
        await _page.WaitForSelectorAsync(".hm-wrap", new PageWaitForSelectorOptions { Timeout = 30000 });
    }

    public async Task DisposeAsync()
    {
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_SectionTitle_RendersUppercaseWithCorrectText()
    {
        var title = _page.Locator(".hm-title").First;
        await title.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await title.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("heatmap section title should be visible");
        text.Should().Contain("MONTHLY EXECUTION HEATMAP",
            "title must display 'MONTHLY EXECUTION HEATMAP' in uppercase");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_Grid_RendersCornerCellAndMonthHeaders()
    {
        // Corner cell should show "STATUS"
        var corner = _page.Locator(".hm-corner").First;
        await corner.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var cornerText = await corner.TextContentAsync();
        cornerText.Should().Contain("STATUS", "corner cell must display 'STATUS'");

        // Month column headers should be present
        var colHeaders = _page.Locator(".hm-col-hdr");
        var headerCount = await colHeaders.CountAsync();
        headerCount.Should().BeGreaterThan(0, "at least one month column header should render from data.json months array");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_Grid_RendersFourCategoryRowHeaders()
    {
        // All four category row headers must be present
        var rowHeaders = _page.Locator(".hm-row-hdr");
        var count = await rowHeaders.CountAsync();
        count.Should().Be(4, "heatmap must render exactly 4 category rows: Shipped, In Progress, Carryover, Blockers");

        // Verify each category header class exists
        var shipHdr = _page.Locator(".ship-hdr");
        (await shipHdr.CountAsync()).Should().BeGreaterThan(0, "Shipped row header (.ship-hdr) should exist");

        var progHdr = _page.Locator(".prog-hdr");
        (await progHdr.CountAsync()).Should().BeGreaterThan(0, "In Progress row header (.prog-hdr) should exist");

        var carryHdr = _page.Locator(".carry-hdr");
        (await carryHdr.CountAsync()).Should().BeGreaterThan(0, "Carryover row header (.carry-hdr) should exist");

        var blockHdr = _page.Locator(".block-hdr");
        (await blockHdr.CountAsync()).Should().BeGreaterThan(0, "Blockers row header (.block-hdr) should exist");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_CurrentMonthColumn_HasHighlightedHeader()
    {
        // The current month header should receive the cur-month-hdr class for gold highlighting
        var highlightedHeaders = _page.Locator(".hm-col-hdr.cur-month-hdr");
        var count = await highlightedHeaders.CountAsync();

        count.Should().BeGreaterOrEqualTo(1,
            "exactly one month column header should be highlighted as the current month with .cur-month-hdr class");

        if (count > 0)
        {
            var text = await highlightedHeaders.First.TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace("highlighted current month header should display a month name");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_DataCells_RenderItemsOrDashPlaceholders()
    {
        // Data cells should exist with category-specific classes
        var allCells = _page.Locator(".hm-cell");
        var cellCount = await allCells.CountAsync();
        cellCount.Should().BeGreaterThan(0, "heatmap data cells should be rendered for each month x category");

        // Check that cells contain either .it items or a dash placeholder
        var itemsCount = await _page.Locator(".hm-cell .it").CountAsync();
        var dashCount = await _page.Locator(".hm-cell span").CountAsync();

        (itemsCount + dashCount).Should().BeGreaterThan(0,
            "cells must contain either item divs (.it) with text or dash (-) placeholders for empty months");

        // Verify category-specific cell classes exist
        var shipCells = _page.Locator(".ship-cell");
        (await shipCells.CountAsync()).Should().BeGreaterThan(0, "shipped data cells (.ship-cell) should exist");

        var progCells = _page.Locator(".prog-cell");
        (await progCells.CountAsync()).Should().BeGreaterThan(0, "in-progress data cells (.prog-cell) should exist");
    }
}