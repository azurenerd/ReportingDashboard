using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Tests for the Heatmap region of Dashboard.razor.
/// Covers: section title, grid structure, category rows, current month highlighting, data cells.
/// </summary>
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
        var corner = _page.Locator(".hm-corner").First;
        await corner.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var cornerText = await corner.TextContentAsync();
        cornerText.Should().Contain("STATUS", "corner cell must display 'STATUS'");

        var colHeaders = _page.Locator(".hm-col-hdr");
        var headerCount = await colHeaders.CountAsync();
        headerCount.Should().BeGreaterThan(0, "at least one month column header should render");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_Grid_RendersFourCategoryRowHeaders()
    {
        var rowHeaders = _page.Locator(".hm-row-hdr");
        var count = await rowHeaders.CountAsync();
        count.Should().Be(4, "heatmap must render exactly 4 category rows: Shipped, In Progress, Carryover, Blockers");

        (await _page.Locator(".ship-hdr").CountAsync()).Should().BeGreaterThan(0, "Shipped row header should exist");
        (await _page.Locator(".prog-hdr").CountAsync()).Should().BeGreaterThan(0, "In Progress row header should exist");
        (await _page.Locator(".carry-hdr").CountAsync()).Should().BeGreaterThan(0, "Carryover row header should exist");
        (await _page.Locator(".block-hdr").CountAsync()).Should().BeGreaterThan(0, "Blockers row header should exist");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_CurrentMonthColumn_HasHighlightedHeader()
    {
        var highlighted = _page.Locator(".hm-col-hdr.cur-month-hdr");
        var count = await highlighted.CountAsync();

        count.Should().BeGreaterOrEqualTo(1,
            "at least one month column header should be highlighted as current month");

        if (count > 0)
        {
            var text = await highlighted.First.TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace("highlighted current month header should display a month name");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_DataCells_RenderItemsOrDashPlaceholders()
    {
        var allCells = _page.Locator(".hm-cell");
        var cellCount = await allCells.CountAsync();
        cellCount.Should().BeGreaterThan(0, "heatmap data cells should be rendered");

        var itemsCount = await _page.Locator(".hm-cell .it").CountAsync();
        var dashCount = await _page.Locator(".hm-cell span").CountAsync();

        (itemsCount + dashCount).Should().BeGreaterThan(0,
            "cells must contain either item divs (.it) or dash (-) placeholders");

        (await _page.Locator(".ship-cell").CountAsync()).Should().BeGreaterThan(0, "shipped cells should exist");
        (await _page.Locator(".prog-cell").CountAsync()).Should().BeGreaterThan(0, "in-progress cells should exist");
        (await _page.Locator(".carry-cell").CountAsync()).Should().BeGreaterThan(0, "carryover cells should exist");
        (await _page.Locator(".block-cell").CountAsync()).Should().BeGreaterThan(0, "blocker cells should exist");
    }
}