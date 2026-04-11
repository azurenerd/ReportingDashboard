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

    // Selectors
    private const string DashboardRootSelector = "div.dashboard-root";
    private const string ErrorPanelSelector = "div.error-panel";
    private const string ErrorTitleSelector = "div.error-title";
    private const string ErrorDetailsSelector = "div.error-details";
    private const string ErrorHelpSelector = "div.error-help";
    private const string HeaderSelector = "div.hdr";
    private const string TimelineSelector = "div.tl-area";
    private const string HeatmapSelector = "div.hm-wrap";

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    public ILocator DashboardRoot => _page.Locator(DashboardRootSelector);
    public ILocator ErrorPanel => _page.Locator(ErrorPanelSelector);
    public ILocator ErrorTitle => _page.Locator(ErrorTitleSelector);
    public ILocator ErrorDetails => _page.Locator(ErrorDetailsSelector);
    public ILocator ErrorHelp => _page.Locator(ErrorHelpSelector);
    public ILocator Header => _page.Locator(HeaderSelector);
    public ILocator Timeline => _page.Locator(TimelineSelector);
    public ILocator Heatmap => _page.Locator(HeatmapSelector);

    public async Task<bool> IsDashboardLoadedAsync()
    {
        var count = await DashboardRoot.CountAsync();
        return count > 0 && await DashboardRoot.IsVisibleAsync();
    }

    public async Task<bool> IsErrorStateAsync()
    {
        var count = await ErrorPanel.CountAsync();
        return count > 0 && await ErrorPanel.IsVisibleAsync();
    }

    public async Task<string> GetPageTitleAsync() =>
        await _page.TitleAsync();

    public async Task<string> GetHeaderTitleTextAsync() =>
        await _page.Locator("div.hdr h1").TextContentAsync() ?? string.Empty;
}