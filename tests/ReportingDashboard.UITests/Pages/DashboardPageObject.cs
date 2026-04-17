using Microsoft.Playwright;

namespace ReportingDashboard.UITests.Pages;

public class DashboardPageObject
{
    private readonly IPage _page;

    public DashboardPageObject(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync(string baseUrl = "http://localhost:5000")
    {
        await _page.GotoAsync(baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<string> GetHeaderTitleAsync()
    {
        var h1 = _page.Locator(".hdr h1");
        await h1.WaitForAsync();
        return await h1.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetSubtitleAsync()
    {
        var subtitle = _page.Locator(".sub");
        await subtitle.WaitForAsync();
        return await subtitle.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetBacklogLinkHrefAsync()
    {
        var link = _page.Locator(".hdr a");
        await link.WaitForAsync();
        return await link.GetAttributeAsync("href") ?? string.Empty;
    }

    public async Task<bool> TimelineAreaVisibleAsync()
    {
        var tlArea = _page.Locator(".tl-area");
        return await tlArea.IsVisibleAsync();
    }

    public async Task<bool> HeatmapAreaVisibleAsync()
    {
        var hmWrap = _page.Locator(".hm-wrap");
        return await hmWrap.IsVisibleAsync();
    }

    public async Task<string> GetHeatmapTitleAsync()
    {
        var title = _page.Locator(".hm-title");
        return await title.TextContentAsync() ?? string.Empty;
    }

    public async Task<int> GetHeatmapColumnCountAsync()
    {
        var grid = _page.Locator(".hm-grid");
        return await grid.CountAsync();
    }

    public async Task<bool> ErrorPanelVisibleAsync()
    {
        var errorPanel = _page.Locator("text=Unable to load dashboard data");
        try
        {
            return await errorPanel.IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetErrorMessageAsync()
    {
        var errorText = _page.Locator("text=/Unable to load dashboard data/");
        return await errorText.TextContentAsync() ?? string.Empty;
    }
}