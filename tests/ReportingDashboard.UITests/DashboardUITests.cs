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

    [Fact]
    public async Task Timeline_RendersWithTrackLabelsAndSvg()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Timeline area should be visible
        var tlArea = page.Locator(".tl-area");
        await Expect(tlArea).ToBeVisibleAsync();

        // Track labels should exist
        var trackLabels = page.Locator(".tl-track-label");
        var count = await trackLabels.CountAsync();
        count.Should().BeGreaterOrEqualTo(1, "at least one track label should render");

        // SVG element should be present
        var svg = page.Locator("svg");
        await Expect(svg).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Heatmap_RendersGridWithMonthHeaders()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Heatmap wrapper should be visible
        var hmWrap = page.Locator(".hm-wrap");
        await Expect(hmWrap).ToBeVisibleAsync();

        // Title should contain expected text
        var title = page.Locator(".hm-title");
        var titleText = await title.TextContentAsync();
        titleText.Should().NotBeNull();
        titleText!.ToUpperInvariant().Should().Contain("MONTHLY EXECUTION HEATMAP");

        // Month column headers should exist
        var colHeaders = page.Locator(".hm-col-hdr");
        var headerCount = await colHeaders.CountAsync();
        headerCount.Should().BeGreaterOrEqualTo(1, "at least one month column header should render");
    }

    [Fact]
    public async Task Heatmap_HighlightedMonthHasSpecialStyling()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Look for the highlighted column header
        var highlighted = page.Locator(".hm-col-highlight");
        var highlightCount = await highlighted.CountAsync();
        highlightCount.Should().BeGreaterOrEqualTo(1, "at least one month should be highlighted");

        var text = await highlighted.First.TextContentAsync();
        text.Should().Contain("Now", "highlighted month should include Now indicator");
    }

    [Fact]
    public async Task HeatmapCells_RenderItemsOrDashes()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Data cells should exist
        var cells = page.Locator(".hm-cell");
        var cellCount = await cells.CountAsync();
        cellCount.Should().BeGreaterOrEqualTo(1, "at least one heatmap cell should render");

        // Each cell should contain .it elements (items or dash)
        var items = page.Locator(".hm-cell .it");
        var itemCount = await items.CountAsync();
        itemCount.Should().BeGreaterOrEqualTo(1, "cells should contain item elements");
    }

    [Fact]
    public async Task Dashboard_FitsViewportWithNoScrollbars()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check that body overflow is hidden (no scrollbars)
        var overflow = await page.EvaluateAsync<string>(
            "() => window.getComputedStyle(document.body).overflow");
        overflow.Should().Be("hidden");
    }

    private static ILocatorAssertions Expect(ILocator locator) =>
        Assertions.Expect(locator);
}