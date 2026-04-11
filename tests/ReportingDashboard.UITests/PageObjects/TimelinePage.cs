using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class TimelinePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public TimelinePage(IPage page, string baseUrl)
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

    // Timeline section
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineLabelsContainer => _page.Locator(".tl-labels");
    public ILocator TimelineLabels => _page.Locator(".tl-label");
    public ILocator TimelineTrackIds => _page.Locator(".tl-id");
    public ILocator TimelineTrackNames => _page.Locator(".tl-name");
    public ILocator SvgBox => _page.Locator(".tl-svg-box");
    public ILocator SvgElement => _page.Locator(".tl-svg-box svg");

    // SVG elements
    public ILocator SvgPolygons => _page.Locator(".tl-svg-box svg polygon");
    public ILocator SvgCircles => _page.Locator(".tl-svg-box svg circle");
    public ILocator SvgTitles => _page.Locator(".tl-svg-box svg title");
    public ILocator SvgFilter => _page.Locator(".tl-svg-box svg defs filter");

    // Grid lines (stroke="#bbb")
    public ILocator GridLines => _page.Locator(".tl-svg-box svg line[stroke='#bbb']");

    // NOW line
    public ILocator NowLine => _page.Locator(".tl-svg-box svg line[stroke='#EA4335']");

    // Track lines (stroke-width="3")
    public ILocator TrackLines => _page.Locator(".tl-svg-box svg line[stroke-width='3']");

    // Dashboard sections
    public ILocator HeaderSection => _page.Locator(".hdr");
    public ILocator HeatmapSection => _page.Locator(".hm-wrap");

    // Error panel
    public ILocator ErrorPanel => _page.Locator("[class*='error'], [class*='Error']");

    public async Task<int> GetTrackLabelCountAsync() =>
        await TimelineLabels.CountAsync();

    public async Task<string> GetTrackIdTextAsync(int index) =>
        await TimelineTrackIds.Nth(index).TextContentAsync() ?? "";

    public async Task<string> GetTrackNameTextAsync(int index) =>
        await TimelineTrackNames.Nth(index).TextContentAsync() ?? "";

    public async Task<string?> GetTrackIdColorAsync(int index) =>
        await TimelineTrackIds.Nth(index).GetAttributeAsync("style");

    public async Task<string?> GetSvgWidthAsync() =>
        await SvgElement.GetAttributeAsync("width");

    public async Task<string?> GetSvgHeightAsync() =>
        await SvgElement.GetAttributeAsync("height");

    public async Task<string?> GetSvgOverflowAsync() =>
        await SvgElement.GetAttributeAsync("overflow");

    public async Task<int> GetPolygonCountAsync() =>
        await SvgPolygons.CountAsync();

    public async Task<int> GetCircleCountAsync() =>
        await SvgCircles.CountAsync();

    public async Task<int> GetGridLineCountAsync() =>
        await GridLines.CountAsync();

    public async Task<bool> IsNowLineVisibleAsync() =>
        await NowLine.CountAsync() > 0;

    public async Task<bool> PageContainsTextAsync(string text)
    {
        var content = await _page.ContentAsync();
        return content.Contains(text);
    }
}