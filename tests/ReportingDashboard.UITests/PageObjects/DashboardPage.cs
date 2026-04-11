using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    private const string HeaderSelector = ".hdr";
    private const string TimelineSelector = ".tl-area";
    private const string HeatmapSelector = ".hm-wrap";
    private const string HeatmapGridSelector = ".hm-grid";
    private const string ErrorPanelSelector = ".error-panel";
    private const string NavSidebarSelector = "nav";
    private const string FooterSelector = "footer";
    private const string CssLinkSelector = "link[rel='stylesheet'][href*='dashboard']";
    private const string BlazorScriptSelector = "script[src*='blazor']";

    public DashboardPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateAsync(string path = "/")
    {
        await _page.GotoAsync($"{_baseUrl}{path}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    public async Task WaitForDashboardLoadedAsync()
    {
        try
        {
            await _page.WaitForSelectorAsync($"{ErrorPanelSelector}, {HeaderSelector}, {HeatmapSelector}", new PageWaitForSelectorOptions
            {
                Timeout = 15_000
            });
        }
        catch (TimeoutException)
        {
            // Page loaded but neither selector found — proceed anyway
        }
    }

    public ILocator Header => _page.Locator(HeaderSelector);
    public ILocator HeaderSection => _page.Locator(HeaderSelector);
    public ILocator Timeline => _page.Locator(TimelineSelector);
    public ILocator TimelineSection => _page.Locator(TimelineSelector);
    public ILocator Heatmap => _page.Locator(HeatmapSelector);
    public ILocator HeatmapSection => _page.Locator(HeatmapSelector);
    public ILocator HeatmapGrid => _page.Locator(HeatmapGridSelector);
    public ILocator ErrorPanel => _page.Locator(ErrorPanelSelector);
    public ILocator CssLink => _page.Locator(CssLinkSelector);
    public ILocator BlazorScript => _page.Locator(BlazorScriptSelector);

    public async Task<bool> IsErrorStateAsync()
    {
        return await ErrorPanel.CountAsync() > 0 && await ErrorPanel.IsVisibleAsync();
    }

    public async Task<bool> IsDashboardContentVisibleAsync()
    {
        var headerVisible = await Header.CountAsync() > 0;
        var heatmapVisible = await Heatmap.CountAsync() > 0;
        return headerVisible || heatmapVisible;
    }

    public async Task<bool> HasNavSidebarAsync()
    {
        return await _page.Locator(NavSidebarSelector).CountAsync() > 0;
    }

    public async Task<bool> HasFooterAsync()
    {
        return await _page.Locator(FooterSelector).CountAsync() > 0;
    }

    public async Task<string> GetTitleAsync()
    {
        return await _page.TitleAsync();
    }

    public IPage Page => _page;
}