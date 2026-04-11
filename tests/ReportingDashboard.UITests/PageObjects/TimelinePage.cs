using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class TimelinePage
{
    private readonly IPage _page;

    public TimelinePage(IPage page)
    {
        _page = page;
    }

    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator SvgBox => _page.Locator(".tl-svg-box");
    public ILocator Svg => _page.Locator(".tl-svg-box svg");

    // SVG milestone markers
    public ILocator PocDiamonds => _page.Locator("polygon[fill='#F4B400']");
    public ILocator ProductionDiamonds => _page.Locator("polygon[fill='#34A853']");
    public ILocator CheckpointCircles => _page.Locator(".tl-svg-box svg circle");

    // NOW line
    public ILocator NowLine => _page.Locator("line[stroke='#EA4335'][stroke-dasharray='5,3']");

    // Drop shadow filter
    public ILocator DropShadowFilter => _page.Locator("filter#sh");

    // Track labels in sidebar
    public ILocator TrackLabels => _page.Locator(".tl-area > div:first-child > div");

    // All SVG text elements
    public ILocator SvgTextElements => _page.Locator(".tl-svg-box svg text");

    public async Task<int> GetTrackCountAsync()
    {
        return await TrackLabels.CountAsync();
    }

    public async Task<string?> GetSvgWidthAsync()
    {
        return await Svg.GetAttributeAsync("width");
    }

    public async Task<string?> GetSvgHeightAsync()
    {
        return await Svg.GetAttributeAsync("height");
    }
}