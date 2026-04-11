using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for the main dashboard page, aligned with the actual PR #535 component structure.
/// </summary>
public class DashboardPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public DashboardPageObject(IPage page, string baseUrl)
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

    // Dashboard root
    public ILocator DashboardRoot => _page.Locator(".dashboard-root");

    // Header section
    public ILocator Header => _page.Locator(".hdr");
    public ILocator Title => _page.Locator(".hdr h1");
    public ILocator Subtitle => _page.Locator(".hdr .sub");
    public ILocator BacklogLink => _page.Locator(".hdr h1 a");
    public ILocator Legend => _page.Locator(".hdr .legend");
    public ILocator LegendItems => _page.Locator(".hdr .legend .legend-item");
    public ILocator LegendPocDiamond => _page.Locator(".legend-diamond.poc");
    public ILocator LegendProdDiamond => _page.Locator(".legend-diamond.prod");
    public ILocator LegendCircle => _page.Locator(".legend-circle");
    public ILocator LegendBar => _page.Locator(".legend-bar");

    // Timeline section
    public ILocator TimelineArea => _page.Locator(".tl-area");
    public ILocator TimelineLabels => _page.Locator(".tl-labels");
    public ILocator TimelineTrackLabels => _page.Locator(".tl-label");
    public ILocator TimelineTrackIds => _page.Locator(".tl-track-id");
    public ILocator TimelineTrackNames => _page.Locator(".tl-track-name");
    public ILocator TimelineSvgBox => _page.Locator(".tl-svg-box");
    public ILocator TimelineSvg => _page.Locator(".tl-svg-box svg");

    // Heatmap section
    public ILocator HeatmapWrap => _page.Locator(".hm-wrap");
    public ILocator HeatmapTitle => _page.Locator(".hm-title");
    public ILocator HeatmapGrid => _page.Locator(".hm-grid");
    public ILocator HeatmapCorner => _page.Locator(".hm-corner");
    public ILocator HeatmapColumnHeaders => _page.Locator(".hm-col-hdr");
    public ILocator HeatmapCurrentMonthHeader => _page.Locator(".hm-col-hdr.cur-hdr");
    public ILocator HeatmapRowHeaders => _page.Locator(".hm-row-hdr");
    public ILocator HeatmapCells => _page.Locator(".hm-cell");
    public ILocator HeatmapCurrentCells => _page.Locator(".hm-cell.cur");
    public ILocator HeatmapItems => _page.Locator(".hm-cell .it");
    public ILocator HeatmapEmptyCells => _page.Locator(".empty-cell");

    // Category-specific selectors
    public ILocator ShippedHeader => _page.Locator(".ship-hdr");
    public ILocator InProgressHeader => _page.Locator(".prog-hdr");
    public ILocator CarryoverHeader => _page.Locator(".carry-hdr");
    public ILocator BlockersHeader => _page.Locator(".block-hdr");
    public ILocator ShippedCells => _page.Locator(".ship-cell");
    public ILocator InProgressCells => _page.Locator(".prog-cell");
    public ILocator CarryoverCells => _page.Locator(".carry-cell");
    public ILocator BlockerCells => _page.Locator(".block-cell");

    // Error panel
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorTitle => _page.Locator(".error-title");
    public ILocator ErrorDetails => _page.Locator(".error-details");
    public ILocator ErrorHelp => _page.Locator(".error-help");
    public ILocator ErrorIcon => _page.Locator(".error-icon");

    // Helpers
    public async Task<bool> IsDashboardVisibleAsync() =>
        await DashboardRoot.CountAsync() > 0;

    public async Task<bool> IsErrorVisibleAsync() =>
        await ErrorPanel.CountAsync() > 0;

    public async Task<int> GetTrackCountAsync() =>
        await TimelineTrackLabels.CountAsync();

    public async Task<int> GetMonthColumnCountAsync() =>
        await HeatmapColumnHeaders.CountAsync();

    public async Task<int> GetHeatmapRowCountAsync() =>
        await HeatmapRowHeaders.CountAsync();
}