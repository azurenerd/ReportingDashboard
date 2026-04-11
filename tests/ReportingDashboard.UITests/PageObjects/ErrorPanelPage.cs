using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class ErrorPanelPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPanelPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl + "/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
    }

    // ── Locators ───────────────────────────────────────────────────────

    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-panel .error-icon");
    public ILocator Title => _page.Locator(".error-panel h2");
    public ILocator MessageParagraph => _page.Locator(".error-panel > p:not(.error-hint)");
    public ILocator HintText => _page.Locator(".error-panel .error-hint");

    // Dashboard sections (should NOT be present when error panel shows)
    public ILocator DashboardHeader => _page.Locator(".hdr");
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");

    // ── Queries ────────────────────────────────────────────────────────

    public async Task<bool> IsErrorPanelVisibleAsync()
    {
        return await ErrorPanel.IsVisibleAsync();
    }

    public async Task<bool> IsDashboardVisibleAsync()
    {
        return await DashboardHeader.IsVisibleAsync();
    }

    public async Task<string> GetTitleTextAsync()
    {
        return await Title.InnerTextAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        return await MessageParagraph.InnerTextAsync();
    }

    public async Task<string> GetHintTextAsync()
    {
        return await HintText.InnerTextAsync();
    }

    public async Task<string> GetIconTextAsync()
    {
        return await ErrorIcon.InnerTextAsync();
    }
}