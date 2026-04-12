using Microsoft.Playwright;

namespace ReportingDashboard.UITests.Pages;

public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Locators
    public ILocator DashboardRoot => _page.Locator(".dashboard-root");
    public ILocator ErrorState => _page.Locator(".error-state");
    public ILocator ErrorDetail => _page.Locator(".error-detail");
    public ILocator Header => _page.Locator(".hdr");
    public ILocator HeaderTitle => _page.Locator(".hdr h1");
    public ILocator HeaderSubtitle => _page.Locator(".hdr .sub");
    public ILocator BacklogLink => _page.Locator(".hdr h1 a");
    public ILocator Legend => _page.Locator(".legend");
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineSidebar => _page.Locator(".tl-sidebar");
    public ILocator TimelineLabels => _page.Locator(".tl-label");
    public ILocator TimelineSvg => _page.Locator(".tl-svg-box svg");
    public ILocator NowLine => _page.Locator("text:has-text('NOW')");
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator CurrentColumnHeader => _page.Locator(".hm-col-hdr.apr-hdr");
    public ILocator HeatmapRowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator HeatmapCells => _page.Locator(".hm-cell");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    public async Task WaitForDashboardAsync()
    {
        await DashboardRoot.WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
    }

    public async Task WaitForErrorStateAsync()
    {
        await ErrorState.WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
    }
}