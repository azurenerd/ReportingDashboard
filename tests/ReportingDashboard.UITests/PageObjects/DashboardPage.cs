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
            Timeout = 30_000
        });
    }

    public ILocator DashboardContainer => _page.Locator(".dashboard-container");
    public ILocator ErrorBanner => _page.Locator(".error-banner");
    public ILocator ProjectHeader => _page.Locator(".section.project-header");

    /// <summary>
    /// Forward-looking locator: requires Timeline component from issue #512.
    /// Will only match once <c>MilestoneTimeline.razor</c> renders <c>.tl-area</c>.
    /// </summary>
    public ILocator TimelineArea => _page.Locator(".tl-area");

    /// <summary>
    /// Forward-looking locator: requires Heatmap component from issue #513.
    /// Will only match once <c>Heatmap.razor</c> renders <c>.hm-wrap</c>.
    /// </summary>
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");

    public ILocator StatusBadge => _page.Locator(".status-badge");
    public ILocator MetricsGrid => _page.Locator(".metrics-grid");
    public ILocator MilestoneTimeline => _page.Locator(".timeline-track");
    public ILocator WorkItems => _page.Locator(".work-item");

    public async Task<string> GetTitleAsync()
    {
        return await _page.TitleAsync();
    }

    public async Task<bool> HasErrorBannerAsync()
    {
        return await ErrorBanner.CountAsync() > 0;
    }

    public async Task<bool> HasDashboardContainerAsync()
    {
        return await DashboardContainer.CountAsync() > 0;
    }
}