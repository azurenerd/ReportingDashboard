using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for granular Heatmap cell and row testing.
/// Covers cell background colors, category theming, current month highlighting,
/// and individual item content within cells.
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

    // Headers
    public ILocator Corner => _page.Locator(".hm-corner");
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator CurrentMonthHeader => _page.Locator(".hm-col-hdr.apr-hdr");
    public ILocator RowHeaders => _page.Locator(".hm-row-hdr");

    // Category-specific headers
    public ILocator ShippedHeader => _page.Locator(".ship-hdr");
    public ILocator InProgressHeader => _page.Locator(".prog-hdr");
    public ILocator CarryoverHeader => _page.Locator(".carry-hdr");
    public ILocator BlockersHeader => _page.Locator(".block-hdr");

    // Category-specific cells
    public ILocator ShippedCells => _page.Locator(".ship-cell");
    public ILocator InProgressCells => _page.Locator(".prog-cell");
    public ILocator CarryoverCells => _page.Locator(".carry-cell");
    public ILocator BlockerCells => _page.Locator(".block-cell");

    // Current month cells (with 'apr' class)
    public ILocator ShippedCurrentCell => _page.Locator(".ship-cell.apr");
    public ILocator InProgressCurrentCell => _page.Locator(".prog-cell.apr");
    public ILocator CarryoverCurrentCell => _page.Locator(".carry-cell.apr");
    public ILocator BlockerCurrentCell => _page.Locator(".block-cell.apr");

    // Items within cells
    public ILocator AllItems => _page.Locator(".hm-cell .it");
    public ILocator ShippedItems => _page.Locator(".ship-cell .it");
    public ILocator InProgressItems => _page.Locator(".prog-cell .it");
    public ILocator CarryoverItems => _page.Locator(".carry-cell .it");
    public ILocator BlockerItems => _page.Locator(".block-cell .it");

    // Empty cells
    public ILocator EmptyCellDashes => _page.Locator(".hm-cell div:has-text('-')");

    // Helpers
    public async Task<string> GetCellBackgroundColorAsync(ILocator cell) =>
        await cell.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

    public async Task<string> GetCellTextColorAsync(ILocator cell) =>
        await cell.EvaluateAsync<string>("el => getComputedStyle(el).color");

    public async Task<List<string>> GetItemTextsAsync(ILocator itemLocator)
    {
        var count = await itemLocator.CountAsync();
        var texts = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var text = await itemLocator.Nth(i).TextContentAsync();
            if (text != null) texts.Add(text.Trim());
        }
        return texts;
    }

    public async Task<int> GetMonthColumnCountAsync() => await ColumnHeaders.CountAsync();
    public async Task<int> GetCategoryRowCountAsync() => await RowHeaders.CountAsync();
}