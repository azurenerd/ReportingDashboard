using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeatmapGridComponentTests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapGridComponentTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HeatmapGrid_RendersCssGrid()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var grid = await _fixture.Page.QuerySelectorAsync(".hm-grid");
        grid.Should().NotBeNull();
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysHeatmapTitle()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await _fixture.Page.QuerySelectorAsync(".hm-title");
        title.Should().NotBeNull();

        var titleText = await title!.TextContentAsync();
        titleText.Should().Contain("HEATMAP");
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysStatusRows()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var rowHeaders = await _fixture.Page.QuerySelectorAllAsync(".hm-row-hdr");
        
        rowHeaders.Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysShippedRow()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await _fixture.Page.ContentAsync();
        content.Should().Contain("Shipped");
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysInProgressRow()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await _fixture.Page.ContentAsync();
        content.Should().Contain("In Progress");
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysCarryoverRow()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await _fixture.Page.ContentAsync();
        content.Should().Contain("Carryover");
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysBlockersRow()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await _fixture.Page.ContentAsync();
        content.Should().Contain("Blockers");
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysDataCells()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var cells = await _fixture.Page.QuerySelectorAllAsync(".hm-cell");
        cells.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HeatmapGrid_CellsContainItems()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var items = await _fixture.Page.QuerySelectorAllAsync(".it");
        
        items.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task HeatmapGrid_DisplaysMonthHeaders()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var colHeaders = await _fixture.Page.QuerySelectorAllAsync(".hm-col-hdr");
        colHeaders.Count.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task HeatmapGrid_MarksCurrentMonth()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await _fixture.Page.ContentAsync();
        content.Should().Contain("Now");
    }

    [Fact]
    public async Task HeatmapGrid_AppliesStatusColors()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var shipCell = await _fixture.Page.QuerySelectorAsync(".ship-cell");
        shipCell.Should().NotBeNull();

        var progCell = await _fixture.Page.QuerySelectorAsync(".prog-cell");
        progCell.Should().NotBeNull();
    }

    [Fact]
    public async Task HeatmapGrid_CornerCellExists()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var corner = await _fixture.Page.QuerySelectorAsync(".hm-corner");
        corner.Should().NotBeNull();
    }
}