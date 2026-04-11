using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPage(IPage page, string baseUrl)
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

    public async Task<string> GetTitleAsync()
    {
        return await _page.TitleAsync();
    }

    public ILocator HeaderSection => _page.Locator(".hdr");
    public ILocator TimelineSection => _page.Locator(".tl-area");
    public ILocator HeatmapSection => _page.Locator(".hm-wrap");
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-icon");
    public ILocator ErrorHeading => _page.Locator(".error-panel h2");
    public ILocator ErrorHint => _page.Locator(".error-hint");
    public ILocator Body => _page.Locator("body");
    public ILocator CssLink => _page.Locator("link[href='css/dashboard.css']");
    public ILocator BlazorScript => _page.Locator("script[src='_framework/blazor.server.js']");
    public ILocator SvgElement => _page.Locator("svg");
    public ILocator TimelineLabels => _page.Locator(".tl-labels .tl-label");
    public ILocator TimelineIds => _page.Locator(".tl-id");
    public ILocator TimelineNames => _page.Locator(".tl-name");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");

    public async Task<bool> HasNavSidebarAsync()
    {
        var sidebar = _page.Locator(".sidebar, nav.sidebar, .nav-menu, [class*='sidebar']");
        return await sidebar.CountAsync() > 0;
    }

    public async Task<bool> HasFooterAsync()
    {
        var footer = _page.Locator("footer");
        return await footer.CountAsync() > 0;
    }

    public async Task WaitForDashboardLoadedAsync()
    {
        // Wait for either the dashboard content or error panel to appear
        await _page.WaitForSelectorAsync(".hdr, .error-panel", new PageWaitForSelectorOptions
        {
            Timeout = 15000
        });
    }
}