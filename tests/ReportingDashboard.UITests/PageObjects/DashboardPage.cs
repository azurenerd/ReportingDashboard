using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPage(IPage page, string baseUrl)
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

    // ── Layout Locators ────────────────────────────────────────────────

    public ILocator Header => _page.Locator(".hdr");
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator ErrorPanel => _page.Locator(".error-panel");

    // ── Header elements ────────────────────────────────────────────────

    public ILocator Title => _page.Locator(".hdr h1");
    public ILocator Subtitle => _page.Locator(".hdr .sub");
    public ILocator BacklogLink => _page.Locator(".hdr a[href]");

    // ── Timeline elements ──────────────────────────────────────────────

    public ILocator TimelineSvg => _page.Locator(".tl-area svg");

    // ── Heatmap elements ───────────────────────────────────────────────

    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");

    // ── State checks ───────────────────────────────────────────────────

    public async Task<bool> IsInErrorStateAsync()
    {
        return await ErrorPanel.IsVisibleAsync();
    }

    public async Task<bool> IsInNormalStateAsync()
    {
        return await Header.IsVisibleAsync();
    }

    public async Task<bool> HasAllSectionsAsync()
    {
        var hasHeader = await Header.IsVisibleAsync();
        var hasTimeline = await TimelineArea.IsVisibleAsync();
        var hasHeatmap = await HeatmapWrap.IsVisibleAsync();
        return hasHeader && hasTimeline && hasHeatmap;
    }

    // ── Screenshot ─────────────────────────────────────────────────────

    public async Task<byte[]> CaptureViewportScreenshotAsync()
    {
        return await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = false
        });
    }
}