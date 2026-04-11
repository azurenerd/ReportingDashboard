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

    public ILocator HeaderSection => _page.Locator(".hdr");
    public ILocator TimelineSection => _page.Locator(".tl-area");
    public ILocator HeatmapSection => _page.Locator(".hm-wrap");

    public async Task<bool> IsFullDashboardVisibleAsync()
    {
        var hdr = await HeaderSection.CountAsync() > 0;
        var tl = await TimelineSection.CountAsync() > 0;
        var hm = await HeatmapSection.CountAsync() > 0;
        return hdr && tl && hm;
    }

    public PageViewportSizeResult? GetViewport()
    {
        return _page.ViewportSize;
    }
}