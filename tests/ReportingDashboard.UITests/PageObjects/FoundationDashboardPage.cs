using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object aligned with the actual PR #539 component structure.
/// Uses CSS selectors matching the real source code (not the Sections/ duplicates).
/// </summary>
public class FoundationDashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public FoundationDashboardPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public IPage Page => _page;

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    // Dashboard root container - actual class is "dashboard" (not "dashboard-root")
    public ILocator DashboardContainer => _page.Locator(".dashboard");

    // Header section
    public ILocator Header => _page.Locator(".hdr");
    public ILocator HeaderTitle => _page.Locator(".hdr h1");
    public ILocator HeaderSubtitle => _page.Locator(".hdr .sub");
    public ILocator BacklogLink => _page.Locator(".hdr h1 a[target='_blank']");

    // Legend - actual Header uses inline styles, no .legend class
    // Legend items are sibling spans inside a flex container
    public ILocator LegendContainer => _page.Locator(".hdr > div:last-child");

    // Timeline area
    public ILocator TimelineArea => _page.Locator(".tl-area");

    // Heatmap area
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");

    // Error panel
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorContent => _page.Locator(".error-content");

    // Static resources
    public ILocator CssLink => _page.Locator("link[href='css/dashboard.css']");
    public ILocator BlazorScript => _page.Locator("script[src*='blazor']");
    public ILocator ViewportMeta => _page.Locator("meta[name='viewport'][content*='1920']");
    public ILocator BaseHref => _page.Locator("base[href='/']");

    // Helpers
    public async Task<bool> IsDashboardVisibleAsync() =>
        await DashboardContainer.CountAsync() > 0;

    public async Task<bool> IsErrorVisibleAsync() =>
        await ErrorPanel.CountAsync() > 0;

    public async Task<string> GetPageTitleAsync() =>
        await _page.TitleAsync();

    public async Task<string> GetBodyWidthAsync() =>
        await _page.EvaluateAsync<string>("() => getComputedStyle(document.body).width");

    public async Task<string> GetBodyHeightAsync() =>
        await _page.EvaluateAsync<string>("() => getComputedStyle(document.body).height");

    public async Task<string> GetBodyOverflowAsync() =>
        await _page.EvaluateAsync<string>("() => getComputedStyle(document.body).overflow");

    public async Task<string> GetBodyFontFamilyAsync() =>
        await _page.EvaluateAsync<string>("() => getComputedStyle(document.body).fontFamily");

    public async Task<bool> HasNavSidebarAsync()
    {
        // Blazor default chrome uses NavMenu, sidebar, etc.
        var nav = _page.Locator("nav.sidebar, .sidebar, .nav-menu, .top-row");
        return await nav.CountAsync() > 0;
    }
}