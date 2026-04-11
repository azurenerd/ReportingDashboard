using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class ErrorPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
    }

    public ILocator ErrorContainer => _page.Locator(".error-container");
    public ILocator ErrorMessage => _page.Locator(".error-message");

    public ILocator Header => _page.Locator(".hdr");
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");

    public IPage Page => _page;

    public async Task<string> GetErrorTextAsync()
    {
        return await ErrorMessage.TextContentAsync() ?? "";
    }

    public async Task<bool> IsErrorVisibleAsync()
    {
        var count = await ErrorContainer.CountAsync();
        return count > 0;
    }

    public async Task<bool> IsDashboardContentHiddenAsync()
    {
        var hdrCount = await Header.CountAsync();
        var tlCount = await TimelineArea.CountAsync();
        var hmCount = await HeatmapWrap.CountAsync();
        return hdrCount == 0 && tlCount == 0 && hmCount == 0;
    }
}