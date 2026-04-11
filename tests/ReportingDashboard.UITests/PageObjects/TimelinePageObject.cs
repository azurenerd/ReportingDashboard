using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for the root Components/Timeline.razor with inline SVG rendering.
/// Covers track sidebar labels, SVG milestone markers, NOW line, and month grid.
/// </summary>
public class TimelinePageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public TimelinePageObject(IPage page, string baseUrl)
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

    // Timeline container
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator SvgBox => _page.Locator(".tl-svg-box");
    public ILocator Svg => _page.Locator(".tl-svg-box svg");

    // Sidebar track labels (inline-styled version)
    public ILocator SidebarContainer => _page.Locator(".tl-area > div:first-child");
    public ILocator SidebarTrackDivs => _page.Locator(".tl-area > div:first-child > div");

    // SVG elements
    public ILocator SvgLines => _page.Locator(".tl-svg-box svg line");
    public ILocator SvgPolygons => _page.Locator(".tl-svg-box svg polygon");
    public ILocator SvgCircles => _page.Locator(".tl-svg-box svg circle");
    public ILocator SvgTexts => _page.Locator(".tl-svg-box svg text");
    public ILocator DropShadowFilter => _page.Locator(".tl-svg-box svg defs filter#sh");

    // NOW marker
    public ILocator NowText => _page.Locator(".tl-svg-box svg text:has-text('NOW')");
    public ILocator NowLine => _page.Locator(".tl-svg-box svg line[stroke='#EA4335']");

    // Milestone markers by color
    public ILocator PocDiamonds => _page.Locator(".tl-svg-box svg polygon[fill='#F4B400']");
    public ILocator ProductionDiamonds => _page.Locator(".tl-svg-box svg polygon[fill='#34A853']");

    // Helpers
    public async Task<bool> IsTimelineVisibleAsync() =>
        await TimelineArea.CountAsync() > 0 && await TimelineArea.IsVisibleAsync();

    public async Task<bool> HasSvgAsync() =>
        await Svg.CountAsync() > 0;

    public async Task<string?> GetSvgWidthAsync() =>
        await Svg.GetAttributeAsync("width");

    public async Task<string?> GetSvgHeightAsync() =>
        await Svg.GetAttributeAsync("height");

    public async Task<int> GetTrackCountAsync() =>
        await SidebarTrackDivs.CountAsync();

    public async Task<int> GetPocDiamondCountAsync() =>
        await PocDiamonds.CountAsync();

    public async Task<int> GetProductionDiamondCountAsync() =>
        await ProductionDiamonds.CountAsync();

    public async Task<int> GetCheckpointCircleCountAsync() =>
        await SvgCircles.CountAsync();

    public async Task<bool> HasNowMarkerAsync() =>
        await NowText.CountAsync() > 0;
}