using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for foundation/scaffolding-level verification.
/// Covers HTML document structure, layout shell, CSS references, and viewport constraints.
/// </summary>
public class FoundationPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public FoundationPageObject(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public IPage Page => _page;

    public async Task NavigateAsync(string path = "/")
    {
        await _page.GotoAsync($"{_baseUrl}{path}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    // Document structure
    public ILocator Html => _page.Locator("html");
    public ILocator Body => _page.Locator("body");
    public ILocator Head => _page.Locator("head");
    public ILocator Title => _page.Locator("title");
    public ILocator BaseHref => _page.Locator("base");
    public ILocator CssLink => _page.Locator("link[href='css/dashboard.css']");
    public ILocator ViewportMeta => _page.Locator("meta[name='viewport']");

    // Layout: no default Blazor chrome
    public ILocator NavSidebar => _page.Locator("nav");
    public ILocator TopRow => _page.Locator(".top-row");
    public ILocator Footer => _page.Locator("footer");

    // Major sections
    public ILocator Dashboard => _page.Locator(".dashboard");
    public ILocator Header => _page.Locator(".hdr");
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorContent => _page.Locator(".error-content");

    // Helpers
    public async Task<string> GetPageTitleAsync() => await _page.TitleAsync();
    public async Task<string> GetLangAttributeAsync() =>
        await Html.GetAttributeAsync("lang") ?? "";
    public async Task<bool> IsDashboardVisibleAsync() =>
        await Dashboard.CountAsync() > 0;
    public async Task<bool> IsErrorVisibleAsync() =>
        await ErrorPanel.CountAsync() > 0;
    public async Task<bool> HasNavSidebarAsync() =>
        await NavSidebar.CountAsync() > 0;
    public async Task<bool> HasFooterAsync() =>
        await Footer.CountAsync() > 0;
    public async Task<bool> HasTopRowAsync() =>
        await TopRow.CountAsync() > 0;
}