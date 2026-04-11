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
    public ILocator SvgElement => _page.Locator(".tl-svg-box svg");
    public ILocator TrackLabels => _page.Locator(".tl-label");
    public ILocator TrackIds => _page.Locator(".tl-id");
    public ILocator TrackNames => _page.Locator(".tl-name");
    public ILocator SvgLines => _page.Locator(".tl-svg-box svg line");
    public ILocator PocDiamonds => _page.Locator("polygon[fill='#F4B400']");
    public ILocator ProductionDiamonds => _page.Locator("polygon[fill='#34A853']");
    public ILocator CheckpointCircles => _page.Locator(".tl-svg-box svg circle");
    public ILocator NowLabel => _page.Locator("text:has-text('NOW')");
    public ILocator NowLine => _page.Locator("line[stroke='#EA4335']");
    public ILocator DropShadowFilter => _page.Locator("filter#shadow");
    public ILocator MilestoneTooltips => _page.Locator(".tl-svg-box svg title");
    public ILocator MonthLabels => _page.Locator(".svg-text text[fill='#666']");

    public async Task<string?> GetSvgWidthAsync()
    {
        return await SvgElement.GetAttributeAsync("width");
    }

    public async Task<string?> GetSvgHeightAsync()
    {
        return await SvgElement.GetAttributeAsync("height");
    }

    public async Task<int> GetTrackCountAsync()
    {
        return await TrackLabels.CountAsync();
    }
}