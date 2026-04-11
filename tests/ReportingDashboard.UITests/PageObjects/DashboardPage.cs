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
            Timeout = 30000
        });
    }

    // Dashboard root
    public ILocator DashboardRoot => _page.Locator(".dashboard-root");

    // Error panel
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-icon");
    public ILocator ErrorTitle => _page.Locator(".error-title");
    public ILocator ErrorDetails => _page.Locator(".error-details");
    public ILocator ErrorHelp => _page.Locator(".error-help");

    // Header
    public ILocator Header => _page.Locator(".hdr");

    // Timeline
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineSvgBox => _page.Locator(".tl-svg-box");
    public ILocator TimelineSvg => _page.Locator(".tl-svg-box svg");
    public ILocator NowLabel => _page.Locator("text >> text=NOW");

    // Heatmap
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapCorner => _page.Locator(".hm-corner");

    // CSS stylesheet
    public ILocator CssLink => _page.Locator("link[href='css/dashboard.css']");
    public ILocator BlazorScript => _page.Locator("script[src='_framework/blazor.server.js']");

    // Body element
    public ILocator Body => _page.Locator("body");

    // SVG elements
    public ILocator SvgPolygons => _page.Locator(".tl-svg-box svg polygon");
    public ILocator SvgCircles => _page.Locator(".tl-svg-box svg circle");
    public ILocator SvgLines => _page.Locator(".tl-svg-box svg line");

    public async Task<string> GetPageTitleAsync()
    {
        return await _page.TitleAsync();
    }

    public async Task<bool> HasDashboardContentAsync()
    {
        return await DashboardRoot.CountAsync() > 0;
    }

    public async Task<bool> HasErrorPanelAsync()
    {
        return await ErrorPanel.CountAsync() > 0;
    }

    public async Task<bool> HasTimelineAsync()
    {
        return await TimelineArea.CountAsync() > 0;
    }

    public async Task<bool> HasHeatmapAsync()
    {
        return await HeatmapWrap.CountAsync() > 0;
    }
}