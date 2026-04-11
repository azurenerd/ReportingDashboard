using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for the root Components/Heatmap.razor with HeatmapRow and HeatmapCell.
/// Uses apr-hdr/apr CSS classes for current month highlighting (not cur-hdr/cur).
/// </summary>
public class HeatmapPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HeatmapPageObject(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public IPage Page => _page;

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    // Heatmap container
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");

    // Header row
    public ILocator CornerCell => _page.Locator(".hm-corner");
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator CurrentMonthHeader => _page.Locator(".hm-col-hdr.apr-hdr");

    // Row headers
    public ILocator RowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator ShippedHeader => _page.Locator(".ship-hdr");
    public ILocator InProgressHeader => _page.Locator(".prog-hdr");
    public ILocator CarryoverHeader => _page.Locator(".carry-hdr");
    public ILocator BlockersHeader => _page.Locator(".block-hdr");

    // Data cells
    public ILocator AllCells => _page.Locator(".hm-cell");
    public ILocator CurrentMonthCells => _page.Locator(".hm-cell.apr");
    public ILocator EmptyCells => _page.Locator(".hm-empty");
    public ILocator ItemDivs => _page.Locator(".hm-cell .it");

    // Category cells
    public ILocator ShippedCells => _page.Locator(".ship-cell");
    public ILocator InProgressCells => _page.Locator(".prog-cell");
    public ILocator CarryoverCells => _page.Locator(".carry-cell");
    public ILocator BlockerCells => _page.Locator(".block-cell");

    // Helpers
    public async Task<bool> IsHeatmapVisibleAsync() =>
        await HeatmapWrap.CountAsync() > 0 && await HeatmapWrap.IsVisibleAsync();

    public async Task<int> GetMonthColumnCountAsync() =>
        await ColumnHeaders.CountAsync();

    public async Task<int> GetRowCountAsync() =>
        await RowHeaders.CountAsync();

    public async Task<int> GetTotalItemCountAsync() =>
        await ItemDivs.CountAsync();

    public async Task<int> GetCurrentMonthCellCountAsync() =>
        await CurrentMonthCells.CountAsync();

    public async Task<bool> HasCurrentMonthHighlightAsync() =>
        await CurrentMonthHeader.CountAsync() > 0;
}