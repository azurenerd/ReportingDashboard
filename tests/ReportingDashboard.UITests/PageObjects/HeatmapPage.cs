using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class HeatmapPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HeatmapPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Navigation
    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    // Title
    public ILocator HeatmapTitle => _page.Locator(".hm-title");

    // Grid
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");

    // Corner cell
    public ILocator CornerCell => _page.Locator(".hm-corner");

    // Column headers
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");

    // Current month header
    public ILocator CurrentMonthHeader => _page.Locator(".hm-col-hdr.cur-month-hdr");

    // Row headers
    public ILocator ShippedRowHeader => _page.Locator("[class*='ship-hdr'], [class*='shipped']").First;
    public ILocator InProgressRowHeader => _page.Locator("[class*='prog-hdr'], [class*='prog']").First;
    public ILocator CarryoverRowHeader => _page.Locator("[class*='carry-hdr'], [class*='carry']").First;
    public ILocator BlockersRowHeader => _page.Locator("[class*='block-hdr'], [class*='block']").First;

    // Data cells
    public ILocator ShippedCells => _page.Locator("[class*='ship-cell']");
    public ILocator InProgressCells => _page.Locator("[class*='prog-cell']");
    public ILocator CarryoverCells => _page.Locator("[class*='carry-cell']");
    public ILocator BlockerCells => _page.Locator("[class*='block-cell']");

    // Entire heatmap wrap
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");

    // Generic helper: get all grid children
    public ILocator GridChildren => _page.Locator(".hm-grid > *");

    // Get column header by index (0-based)
    public ILocator GetColumnHeader(int index) => ColumnHeaders.Nth(index);

    // Get inline style of grid
    public async Task<string?> GetGridStyleAsync()
    {
        return await HeatmapGrid.GetAttributeAsync("style");
    }

    // Get text of title
    public async Task<string> GetTitleTextAsync()
    {
        return await HeatmapTitle.InnerTextAsync();
    }

    // Check if page loaded without error
    public async Task<bool> IsErrorStateAsync()
    {
        var errorPanel = _page.Locator(".error-panel, .err-panel, [class*='error']");
        return await errorPanel.CountAsync() > 0;
    }

    // Take screenshot
    public async Task ScreenshotAsync(string name)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = Path.Combine(dir, $"{name}.png"),
            FullPage = true
        });
    }
}