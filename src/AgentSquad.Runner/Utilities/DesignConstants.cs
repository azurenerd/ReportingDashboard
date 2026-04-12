namespace AgentSquad.Runner.Utilities;

public static class DesignConstants
{
    // Colors
    public const string ColorShipped = "#1B7A28";
    public const string ColorProgress = "#1565C0";
    public const string ColorCarryover = "#B45309";
    public const string ColorBlocker = "#991B1B";
    public const string ColorPoc = "#F4B400";
    public const string ColorProduction = "#34A853";
    public const string ColorNow = "#EA4335";
    public const string ColorPrimary = "#0078D4";

    // Background colors
    public const string BgShipped = "#E8F5E9";
    public const string BgProgress = "#E3F2FD";
    public const string BgCarryover = "#FFF8E1";
    public const string BgBlocker = "#FEF2F2";
    public const string BgShippedCell = "#F0FBF0";
    public const string BgProgressCell = "#EEF4FE";
    public const string BgCarryoverCell = "#FFFDE7";
    public const string BgBlockerCell = "#FFF5F5";
    public const string BgAprilHighlight = "#FFF0D0";

    // CSS classes
    public const string CssShipHdr = "ship-hdr";
    public const string CssProgHdr = "prog-hdr";
    public const string CssCarryHdr = "carry-hdr";
    public const string CssBlockHdr = "block-hdr";

    public const string CssShipCell = "ship-cell";
    public const string CssProgCell = "prog-cell";
    public const string CssCarryCell = "carry-cell";
    public const string CssBlockCell = "block-cell";

    // Typography
    public const int FontSizePageTitle = 24;
    public const int FontSizeSubtitle = 12;
    public const int FontSizeHeatmapTitle = 14;
    public const int FontSizeColHeader = 16;
    public const int FontSizeRowHeader = 11;
    public const int FontSizeCellContent = 12;
    public const int FontSizeTimelineMonth = 11;
    public const int FontSizeMarkerLabel = 10;

    // Grid layout
    public const int HeatmapRowHeaderWidth = 160;
    public const int HeatmapHeaderRowHeight = 36;
    public const int HeatmapDataRowHeight = 80;
    public const string HeatmapColumnTemplate = "160px repeat(4, 1fr)";
    public const string HeatmapRowTemplate = "36px repeat(4, 1fr)";

    // Timeline
    public const int TimelineMonthWidth = 260;
    public const int TimelineSvgWidth = 1560;
    public const int TimelineSvgHeight = 185;
    public const int TimelineMonthCount = 6;

    // Layout
    public const int HeaderHeightPx = 60;
    public const int TimelineHeightPx = 196;
    public const int ViewportWidth = 1920;
    public const int ViewportHeight = 1080;

    public static string GetColorByStatus(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => ColorShipped,
            "in progress" => ColorProgress,
            "carryover" => ColorCarryover,
            "blockers" => ColorBlocker,
            _ => ColorProgress
        };
    }

    public static string GetCssClassByStatus(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => CssShipHdr,
            "in progress" => CssProgHdr,
            "carryover" => CssCarryHdr,
            "blockers" => CssBlockHdr,
            _ => CssProgHdr
        };
    }

    public static string GetBgColorByStatus(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => BgShipped,
            "in progress" => BgProgress,
            "carryover" => BgCarryover,
            "blockers" => BgBlocker,
            _ => BgProgress
        };
    }

    public static string GetCellBgColorByStatus(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => BgShippedCell,
            "in progress" => BgProgressCell,
            "carryover" => BgCarryoverCell,
            "blockers" => BgBlockerCell,
            _ => BgProgressCell
        };
    }

    public static string GetHeaderCssClassByStatus(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => CssShipHdr,
            "in progress" => CssProgHdr,
            "carryover" => CssCarryHdr,
            "blockers" => CssBlockHdr,
            _ => CssProgHdr
        };
    }

    public static string GetCellCssClassByStatus(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => CssShipCell,
            "in progress" => CssProgCell,
            "carryover" => CssCarryCell,
            "blockers" => CssBlockCell,
            _ => CssProgCell
        };
    }
}