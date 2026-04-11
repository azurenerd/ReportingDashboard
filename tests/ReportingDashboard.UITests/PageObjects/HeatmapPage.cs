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

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    // Row headers
    public ILocator ShipHeader => _page.Locator(".ship-hdr");
    public ILocator ProgHeader => _page.Locator(".prog-hdr");
    public ILocator CarryHeader => _page.Locator(".carry-hdr");
    public ILocator BlockHeader => _page.Locator(".block-hdr");
    public ILocator AllRowHeaders => _page.Locator(".hm-row-hdr");

    // Cells by category
    public ILocator ShipCells => _page.Locator(".ship-cell");
    public ILocator ProgCells => _page.Locator(".prog-cell");
    public ILocator CarryCells => _page.Locator(".carry-cell");
    public ILocator BlockCells => _page.Locator(".block-cell");
    public ILocator AllCells => _page.Locator(".hm-cell");

    // Current month highlighted cells
    public ILocator CurrentMonthShipCell => _page.Locator(".ship-cell.apr");
    public ILocator CurrentMonthProgCell => _page.Locator(".prog-cell.apr");
    public ILocator CurrentMonthCarryCell => _page.Locator(".carry-cell.apr");
    public ILocator CurrentMonthBlockCell => _page.Locator(".block-cell.apr");
    public ILocator AllCurrentMonthCells => _page.Locator(".hm-cell.apr");

    // Items within cells
    public ILocator AllItems => _page.Locator(".hm-cell .it");
    public ILocator ShipItems => _page.Locator(".ship-cell .it");
    public ILocator ProgItems => _page.Locator(".prog-cell .it");
    public ILocator CarryItems => _page.Locator(".carry-cell .it");
    public ILocator BlockItems => _page.Locator(".block-cell .it");

    // Empty cell dashes
    public ILocator EmptyDashes => _page.Locator(".hm-cell > div[style*='color:#AAA']");

    // Heatmap wrapper and grid
    public ILocator HeatmapWrapper => _page.Locator(".hm-wrap");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");

    // Column headers
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator CurrentMonthColumnHeader => _page.Locator(".hm-col-hdr.apr-hdr");

    public async Task<string> GetCellBackgroundColorAsync(ILocator cell)
    {
        return await cell.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).backgroundColor");
    }

    public async Task<string> GetElementColorAsync(ILocator element)
    {
        return await element.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).color");
    }

    public async Task<string> GetElementFontSizeAsync(ILocator element)
    {
        return await element.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).fontSize");
    }

    public async Task<string> GetElementFontWeightAsync(ILocator element)
    {
        return await element.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).fontWeight");
    }

    public async Task<string> GetPseudoElementBackgroundAsync(ILocator element)
    {
        return await element.EvaluateAsync<string>(
            "el => window.getComputedStyle(el, '::before').backgroundColor");
    }
}