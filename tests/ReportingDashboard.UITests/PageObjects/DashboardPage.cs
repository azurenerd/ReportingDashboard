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
    public ILocator ErrorSection => _page.Locator(".dashboard-error");
    public ILocator ErrorContent => _page.Locator(".dashboard-error-content");
    public ILocator ErrorHeading => _page.Locator(".dashboard-error-content h2");
    public ILocator ErrorMessage => _page.Locator(".dashboard-error-content p");

    // Header section
    public ILocator Header => _page.Locator(".hdr");
    public ILocator HeaderTitle => _page.Locator(".hdr h1");
    public ILocator HeaderSubtitle => _page.Locator(".hdr .sub");

    // Timeline section
    public ILocator TimelineArea => _page.Locator(".tl-area");

    // Heatmap section
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");

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

    public async Task<ViewportSize?> GetViewportAsync()
    {
        return _page.ViewportSize;
    }
}