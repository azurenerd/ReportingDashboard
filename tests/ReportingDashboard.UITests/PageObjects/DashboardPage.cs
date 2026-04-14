using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page Object for the main dashboard view.
/// Encapsulates selectors and interactions for the executive reporting dashboard.
/// </summary>
public class DashboardPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    // Navigation
    public async Task<IResponse?> NavigateAsync()
    {
        return await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    // Header selectors
    public ILocator Header => _page.Locator(".hdr");
    public ILocator Title => _page.Locator("h1");
    public ILocator Subtitle => _page.Locator(".sub");
    public ILocator BacklogLink => _page.Locator("a[target='_blank']");

    // Timeline selectors
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineSvg => _page.Locator(".tl-svg-box svg");
    public ILocator TrackLabels => _page.Locator(".tl-area div").First;

    // Heatmap selectors
    public ILocator HeatmapWrapper => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapCorner => _page.Locator(".hm-corner");
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator RowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator DataCells => _page.Locator(".hm-cell");

    // Status row selectors
    public ILocator ShippedHeader => _page.Locator(".ship-hdr");
    public ILocator InProgressHeader => _page.Locator(".prog-hdr");
    public ILocator CarryoverHeader => _page.Locator(".carry-hdr");
    public ILocator BlockersHeader => _page.Locator(".block-hdr");

    // General queries
    public async Task<string> GetTitleTextAsync()
    {
        return await Title.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetSubtitleTextAsync()
    {
        return await Subtitle.TextContentAsync() ?? string.Empty;
    }

    public async Task<bool> IsFullyLoadedAsync()
    {
        // Check that key sections exist
        var hasHeader = await Header.CountAsync() > 0;
        var hasTimeline = await TimelineArea.CountAsync() > 0;
        var hasHeatmap = await HeatmapWrapper.CountAsync() > 0;
        return hasHeader && hasTimeline && hasHeatmap;
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