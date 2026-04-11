using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for granular Timeline SVG element testing.
/// Covers milestone shapes, colors, track lines, NOW marker, and month labels.
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

    // Timeline area
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator SvgBox => _page.Locator(".tl-svg-box");
    public ILocator Svg => _page.Locator(".tl-svg-box svg");

    // Track labels sidebar
    public ILocator TrackSidebar => _page.Locator(".tl-area > div:first-child");

    // SVG elements
    public ILocator AllPolygons => _page.Locator(".tl-svg-box svg polygon");
    public ILocator AllCircles => _page.Locator(".tl-svg-box svg circle");
    public ILocator AllLines => _page.Locator(".tl-svg-box svg line");
    public ILocator AllText => _page.Locator(".tl-svg-box svg text");
    public ILocator DropShadowFilter => _page.Locator(".tl-svg-box svg defs filter");

    // NOW marker (dashed line + text)
    public ILocator NowText => _page.Locator(".tl-svg-box svg text:has-text('NOW')");
    public ILocator DashedLines => _page.Locator(".tl-svg-box svg line[stroke-dasharray]");

    // Milestone tooltips (title elements inside shapes)
    public ILocator MilestoneTitles => _page.Locator(".tl-svg-box svg title");

    // Helpers
    public async Task<int> GetPolygonCountAsync() => await AllPolygons.CountAsync();
    public async Task<int> GetCircleCountAsync() => await AllCircles.CountAsync();
    public async Task<int> GetLineCountAsync() => await AllLines.CountAsync();

    public async Task<string?> GetSvgWidthAsync() => await Svg.GetAttributeAsync("width");
    public async Task<string?> GetSvgHeightAsync() => await Svg.GetAttributeAsync("height");

    public async Task<List<string>> GetPolygonFillColorsAsync()
    {
        var count = await AllPolygons.CountAsync();
        var colors = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var fill = await AllPolygons.Nth(i).GetAttributeAsync("fill");
            if (fill != null) colors.Add(fill);
        }
        return colors;
    }

    public async Task<List<string>> GetCircleStrokeColorsAsync()
    {
        var count = await AllCircles.CountAsync();
        var colors = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var stroke = await AllCircles.Nth(i).GetAttributeAsync("stroke");
            if (stroke != null) colors.Add(stroke);
        }
        return colors;
    }

    public async Task<List<string>> GetMilestoneTooltipsAsync()
    {
        var count = await MilestoneTitles.CountAsync();
        var tooltips = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var text = await MilestoneTitles.Nth(i).TextContentAsync();
            if (text != null) tooltips.Add(text);
        }
        return tooltips;
    }
}