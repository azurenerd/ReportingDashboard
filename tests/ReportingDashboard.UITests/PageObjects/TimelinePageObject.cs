using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page Object encapsulating all Timeline-related locators and actions
/// for the Executive Reporting Dashboard.
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

    // Navigation
    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    // Top-level timeline container
    public ILocator TimelineArea => _page.Locator(".tl-area");

    // Sidebar
    public ILocator Sidebar => _page.Locator(".tl-sidebar");
    public ILocator TrackLabels => _page.Locator(".tl-track-label");
    public ILocator TrackIds => _page.Locator(".tl-track-id");
    public ILocator TrackNames => _page.Locator(".tl-track-name");

    // SVG area
    public ILocator SvgBox => _page.Locator(".tl-svg-box");
    public ILocator Svg => _page.Locator(".tl-svg-box svg");

    // SVG elements
    public ILocator MonthGridLines => Svg.Locator("line[stroke='#bbb']");
    public ILocator MonthLabels => Svg.Locator("text[fill='#666'][font-size='11']");
    public ILocator TrackLines => Svg.Locator("line[stroke-width='3']");
    public ILocator NowLine => Svg.Locator("line[stroke='#EA4335'][stroke-dasharray='5,3']");
    public ILocator NowLabel => Svg.Locator("text:has-text('NOW')");
    public ILocator PocDiamonds => Svg.Locator("polygon[fill='#F4B400']");
    public ILocator ProductionDiamonds => Svg.Locator("polygon[fill='#34A853']");
    public ILocator CheckpointCircles => Svg.Locator("circle[fill='white']");
    public ILocator DotCircles => Svg.Locator("circle[fill='#999']");
    public ILocator AllPolygons => Svg.Locator("polygon");
    public ILocator AllCircles => Svg.Locator("circle");
    public ILocator DropShadowFilter => Svg.Locator("filter#sh");
    public ILocator FeDropShadow => Svg.Locator("feDropShadow");
    public ILocator MilestoneDateLabels => Svg.Locator("text[font-size='10'][text-anchor='middle']");

    // Helper methods
    public async Task<int> GetTrackCountAsync() => await TrackLabels.CountAsync();

    public async Task<string?> GetSvgWidthAsync() => await Svg.GetAttributeAsync("width");

    public async Task<string?> GetSvgHeightAsync() => await Svg.GetAttributeAsync("height");

    public async Task<string> GetTrackIdTextAsync(int index) =>
        await TrackIds.Nth(index).TextContentAsync() ?? "";

    public async Task<string> GetTrackNameTextAsync(int index) =>
        await TrackNames.Nth(index).TextContentAsync() ?? "";

    public async Task<string> GetTrackIdColorAsync(int index) =>
        await TrackIds.Nth(index).EvaluateAsync<string>("el => getComputedStyle(el).color");

    public async Task<string> GetTimelineAreaBackgroundAsync() =>
        await TimelineArea.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

    public async Task<string> GetTimelineAreaHeightAsync() =>
        await TimelineArea.EvaluateAsync<string>("el => getComputedStyle(el).height");

    public async Task<string> GetTimelineAreaBorderBottomAsync() =>
        await TimelineArea.EvaluateAsync<string>("el => getComputedStyle(el).borderBottom");

    public async Task<double> GetSidebarComputedWidthAsync() =>
        await Sidebar.EvaluateAsync<double>("el => el.getBoundingClientRect().width");
}