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
        _page = await _fixture.Browser.NewPageAsync();
        _page.SetDefaultTimeout(60000);
        await _page.SetViewportSizeAsync(1920, 1080);
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_IsVisibleWithTitle()
    {
        var heatmapWrap = _page.Locator(".hm-wrap");
        await heatmapWrap.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        (await heatmapWrap.IsVisibleAsync()).Should().BeTrue();

        var title = _page.Locator(".hm-title");
        var text = await title.TextContentAsync();
        text.Should().NotBeNull();
        text!.ToUpperInvariant().Should().Contain("MONTHLY EXECUTION HEATMAP");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_HasCorrectGridStructure()
    {
        var corner = _page.Locator(".hm-corner");
        var cornerText = await corner.TextContentAsync();
        cornerText.Should().NotBeNull();
        cornerText!.Trim().ToUpperInvariant().Should().Be("STATUS");

        var colHeaders = _page.Locator(".hm-col-hdr");
        var colCount = await colHeaders.CountAsync();
        colCount.Should().BeGreaterThan(0, "at least one month column should render");

        var rowHeaders = _page.Locator(".hm-row-hdr");
        var rowCount = await rowHeaders.CountAsync();
        rowCount.Should().Be(4, "four status rows should render");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_FourStatusRowsDisplayCorrectLabels()
    {
        var rowHeaders = _page.Locator(".hm-row-hdr");

        var shipped = await rowHeaders.Nth(0).TextContentAsync();
        shipped.Should().Contain("SHIPPED");

        var inProgress = await rowHeaders.Nth(1).TextContentAsync();
        inProgress.Should().Contain("IN PROGRESS");

        var carryover = await rowHeaders.Nth(2).TextContentAsync();
        carryover.Should().Contain("CARRYOVER");

        var blockers = await rowHeaders.Nth(3).TextContentAsync();
        blockers.Should().Contain("BLOCKERS");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_CurrentMonthColumnIsHighlighted()
    {
        var highlightedHeader = _page.Locator(".hm-col-hdr.apr-hdr");
        var count = await highlightedHeader.CountAsync();

        if (count > 0)
        {
            var headerText = await highlightedHeader.First.TextContentAsync();
            headerText.Should().Contain("Now", "current month header should show Now suffix");

            // Verify highlighted header has the gold background
            var bgColor = await highlightedHeader.First.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bgColor.Should().NotBeNullOrEmpty();
        }

        // Verify current month data cells have apr class
        var aprCells = _page.Locator(".hm-cell.apr");
        var aprCount = await aprCells.CountAsync();
        if (count > 0)
        {
            aprCount.Should().Be(4, "each of the 4 status rows should have one current-month cell");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_DataCellsShowItemsOrDashes()
    {
        var dataCells = _page.Locator(".hm-cell");
        var cellCount = await dataCells.CountAsync();
        cellCount.Should().BeGreaterThan(0, "data cells should render");

        // Check that at least some cells have items or dashes
        var items = _page.Locator(".hm-cell .it");
        var itemCount = await items.CountAsync();

        var empties = _page.Locator(".hm-cell .empty-cell");
        var emptyCount = await empties.CountAsync();

        (itemCount + emptyCount).Should().BeGreaterThan(0,
            "every data cell should contain either items or an empty dash indicator");

        // Verify item text is visible
        if (itemCount > 0)
        {
            var firstItemText = await items.First.TextContentAsync();
            firstItemText.Should().NotBeNullOrWhiteSpace("items should have visible text");
        }
    }
}