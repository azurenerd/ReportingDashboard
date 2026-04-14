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
    public ILocator Header => _page.Locator(".hdr");
    public ILocator Title => _page.Locator("h1");
    public ILocator Subtitle => _page.Locator(".sub");
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator DashboardContainer => _page.Locator(".dashboard");
    public ILocator ErrorBanner => _page.Locator(".error-panel");
    public ILocator StatusBadge => _page.Locator(".status-badge");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<bool> HasDashboardContainerAsync()
    {
        return await DashboardContainer.CountAsync() > 0;
    }

    public async Task<bool> HasErrorBannerAsync()
    {
        return await ErrorBanner.CountAsync() > 0;
    }

    public async Task<string> GetTitleAsync()
    {
        return await Title.InnerTextAsync();
    }

    public async Task<byte[]> TakeScreenshotAsync()
    {
        return await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true,
            Type = ScreenshotType.Png
        });
    }
}