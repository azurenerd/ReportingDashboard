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

    // Navigation
    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    // Header section
    public ILocator Header => _page.Locator(".hdr");
    public ILocator Title => _page.Locator(".hdr h1");
    public ILocator Subtitle => _page.Locator(".hdr .sub");
    public ILocator BacklogLink => _page.Locator(".hdr a[href]");
    public ILocator HeaderLeft => _page.Locator(".hdr-left");
    public ILocator HeaderRight => _page.Locator(".hdr-right");

    // Legend
    public ILocator LegendItems => _page.Locator(".legend-item");
    public ILocator LegendDiamonds => _page.Locator(".legend-diamond");
    public ILocator LegendCircle => _page.Locator(".legend-circle");
    public ILocator LegendNowBar => _page.Locator(".legend-now-bar");

    // Timeline section
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineLabels => _page.Locator(".tl-labels");
    public ILocator TrackLabels => _page.Locator(".tl-label");
    public ILocator TrackIds => _page.Locator(".tl-id");
    public ILocator TrackNames => _page.Locator(".tl-name");
    public ILocator SvgBox => _page.Locator(".tl-svg-box");
    public ILocator Svg => _page.Locator(".tl-svg-box svg");
    public ILocator SvgPolygons => _page.Locator(".tl-svg-box svg polygon");
    public ILocator SvgCircles => _page.Locator(".tl-svg-box svg circle");

    // Heatmap section
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapCorner => _page.Locator(".hm-corner");
    public ILocator ColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator CurrentMonthHeader => _page.Locator(".cur-month-hdr");
    public ILocator RowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator HeatmapCells => _page.Locator(".hm-cell");
    public ILocator HeatmapItems => _page.Locator(".hm-cell .it");
    public ILocator EmptyCells => _page.Locator(".empty-cell");

    // Category-specific
    public ILocator ShippedHeader => _page.Locator(".shipped-hdr");
    public ILocator ProgHeader => _page.Locator(".prog-hdr");
    public ILocator CarryHeader => _page.Locator(".carry-hdr");
    public ILocator BlockHeader => _page.Locator(".block-hdr");

    // Error state
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-icon");
    public ILocator ErrorHeading => _page.Locator(".error-panel h2");
    public ILocator ErrorHint => _page.Locator(".error-hint");

    // Raw page access for custom queries
    public IPage Page => _page;

    public async Task WaitForDashboardLoadedAsync()
    {
        // Wait for either error panel or dashboard content
        await _page.WaitForSelectorAsync(".hdr, .error-panel", new PageWaitForSelectorOptions
        {
            Timeout = 15000
        });
    }
}