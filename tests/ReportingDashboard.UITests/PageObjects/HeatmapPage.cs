using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class HeatmapPage
{
    private readonly IPage _page;

    public HeatmapPage(IPage page)
    {
        _page = page;
    }

    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator Corner => _page.Locator(".hm-corner");
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator CurrentMonthHeader => _page.Locator(".hm-col-hdr.apr-hdr");
    public ILocator RowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator ShippedHeader => _page.Locator(".ship-hdr");
    public ILocator InProgressHeader => _page.Locator(".prog-hdr");
    public ILocator CarryoverHeader => _page.Locator(".carry-hdr");
    public ILocator BlockersHeader => _page.Locator(".block-hdr");
    public ILocator ShippedCells => _page.Locator(".ship-cell");
    public ILocator InProgressCells => _page.Locator(".prog-cell");
    public ILocator CarryoverCells => _page.Locator(".carry-cell");
    public ILocator BlockerCells => _page.Locator(".block-cell");
    public ILocator AllWorkItems => _page.Locator(".hm-cell .it");
    public ILocator CurrentMonthShippedCell => _page.Locator(".ship-cell.apr");
    public ILocator CurrentMonthProgressCell => _page.Locator(".prog-cell.apr");
    public ILocator CurrentMonthCarryoverCell => _page.Locator(".carry-cell.apr");
    public ILocator CurrentMonthBlockerCell => _page.Locator(".block-cell.apr");
}