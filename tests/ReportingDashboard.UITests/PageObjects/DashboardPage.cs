using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    // Navigation
    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
    }

    // Container
    public ILocator MainContainer =>
        _page.Locator("div[style*='width:1920px'][style*='height:1080px']");

    // Error section
    public ILocator ErrorSection => _page.Locator(".error-container");
    public ILocator ErrorMessage => _page.Locator(".error-message");

    // Header section
    public ILocator Header => _page.Locator(".hdr");
    public ILocator HeaderTitle => _page.Locator(".hdr h1");
    public ILocator HeaderSubtitle => _page.Locator(".hdr .sub");
    public ILocator HeaderLeft => _page.Locator(".hdr-left");

    // Timeline section
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineLabels => _page.Locator(".tl-labels");
    public ILocator TimelineSvgBox => _page.Locator(".tl-svg-box");

    // Heatmap section
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapCorner => _page.Locator(".hm-corner");
    public ILocator HeatmapColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator HeatmapRowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator HeatmapCells => _page.Locator(".hm-cell");
    public ILocator HeatmapItems => _page.Locator(".hm-cell .it");
    public ILocator NowBadge => _page.Locator(".now-badge");
    public ILocator NowHeaders => _page.Locator(".now-hdr");

    // General
    public IPage Page => _page;

    public async Task<string> GetTitleAsync() => await _page.TitleAsync();

    public async Task<string> GetPageHtmlAsync() => await _page.ContentAsync();

    public async Task<bool> HasBlazorReconnectModalAsync()
    {
        var modal = _page.Locator(".components-reconnect-modal");
        var count = await modal.CountAsync();
        if (count == 0) return false;
        return await modal.IsVisibleAsync();
    }
}