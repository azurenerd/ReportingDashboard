namespace AgentSquad.Runner.Config;

/// <summary>
/// Centralized visualization constants for colors, CSS classes, SVG dimensions, and status mappings.
/// All visual styling parameters are defined here to ensure consistency across components.
/// </summary>
public static class VisualizationConstants
{
    // ============================================================================
    // Shipped Status Colors
    // ============================================================================
    public const string ColorShippedCellDefault = "#F0FBF0";
    public const string ColorShippedCellCurrentMonth = "#D8F2DA";
    public const string ColorShippedHeaderBackground = "#E8F5E9";
    public const string ColorShippedHeaderText = "#1B7A28";
    public const string ColorShippedDot = "#34A853";

    // ============================================================================
    // In Progress Status Colors
    // ============================================================================
    public const string ColorInProgressCellDefault = "#EEF4FE";
    public const string ColorInProgressCellCurrentMonth = "#DAE8FB";
    public const string ColorInProgressHeaderBackground = "#E3F2FD";
    public const string ColorInProgressHeaderText = "#1565C0";
    public const string ColorInProgressDot = "#0078D4";

    // ============================================================================
    // Carryover Status Colors
    // ============================================================================
    public const string ColorCarryoverCellDefault = "#FFFDE7";
    public const string ColorCarryoverCellCurrentMonth = "#FFF0B0";
    public const string ColorCarryoverHeaderBackground = "#FFF8E1";
    public const string ColorCarryoverHeaderText = "#B45309";
    public const string ColorCarryoverDot = "#F4B400";

    // ============================================================================
    // Blockers Status Colors
    // ============================================================================
    public const string ColorBlockersCellDefault = "#FFF5F5";
    public const string ColorBlockersCellCurrentMonth = "#FFE4E4";
    public const string ColorBlockersHeaderBackground = "#FEF2F2";
    public const string ColorBlockersHeaderText = "#991B1B";
    public const string ColorBlockersDot = "#EA4335";

    // ============================================================================
    // Timeline and Milestone Colors
    // ============================================================================
    public const string ColorTimelineM1 = "#0078D4";
    public const string ColorTimelineM2 = "#00897B";
    public const string ColorTimelineM3 = "#546E7A";
    public const string ColorMilestonePoCDiamond = "#F4B400";
    public const string ColorMilestoneProductionDiamond = "#34A853";
    public const string ColorMilestoneCheckpointCircle = "#999";
    public const string ColorNowMarker = "#EA4335";

    // ============================================================================
    // General UI Colors
    // ============================================================================
    public const string ColorPrimaryText = "#111";
    public const string ColorSecondaryText = "#888";
    public const string ColorTertiaryText = "#666";
    public const string ColorBodyText = "#333";
    public const string ColorLinkText = "#0078D4";
    public const string ColorBorderLight = "#E0E0E0";
    public const string ColorBorderHeavy = "#CCC";
    public const string ColorHeaderBackground = "#F5F5F5";
    public const string ColorPageBackground = "#FFFFFF";
    public const string ColorTimelineBackground = "#FAFAFA";
    public const string ColorCurrentMonthHeaderBackground = "#FFF0D0";
    public const string ColorCurrentMonthHeaderText = "#C07700";

    // ============================================================================
    // CSS Class Names - Status Cells
    // ============================================================================
    public const string CssClassShippedCell = "ship-cell";
    public const string CssClassInProgressCell = "prog-cell";
    public const string CssClassCarryoverCell = "carry-cell";
    public const string CssClassBlockersCell = "block-cell";

    // ============================================================================
    // CSS Class Names - Row Headers
    // ============================================================================
    public const string CssClassShippedHeaderRow = "ship-hdr";
    public const string CssClassInProgressHeaderRow = "prog-hdr";
    public const string CssClassCarryoverHeaderRow = "carry-hdr";
    public const string CssClassBlockersHeaderRow = "block-hdr";

    // ============================================================================
    // CSS Class Names - General
    // ============================================================================
    public const string CssClassCurrentMonth = "apr";
    public const string CssClassCurrentMonthHeader = "apr-hdr";
    public const string CssClassItemWithDot = "it";

    // ============================================================================
    // SVG Dimension Constants
    // ============================================================================
    public const int MilestoneStartCircleRadius = 7;
    public const int MilestoneCheckpointCircleRadius = 4;
    public const int TimelineLineStrokeWidth = 3;
    public const int NowMarkerLineStrokeWidth = 2;
    public const int GridlineStrokeWidth = 1;
    public const double GridlineOpacity = 0.4;

    // ============================================================================
    // Layout Constants
    // ============================================================================
    public const int CanvasWidth = 1920;
    public const int CanvasHeight = 1080;
    public const int HeaderHeight = 60;
    public const int TimelineHeight = 196;
    public const int TimelinePixelsPerMonth = 260;
    public const int SvgTimelineWidth = 1560;
    public const int SvgTimelineHeight = 185;

    // ============================================================================
    // Status Type Constants
    // ============================================================================
    public const string StatusShipped = "shipped";
    public const string StatusInProgress = "inProgress";
    public const string StatusCarryover = "carryover";
    public const string StatusBlockers = "blockers";

    // ============================================================================
    // Milestone Type Constants
    // ============================================================================
    public const string MilestoneTypePoC = "poc";
    public const string MilestoneTypeRelease = "release";
    public const string MilestoneTypeCheckpoint = "checkpoint";

    // ============================================================================
    // Utility Methods
    // ============================================================================

    /// <summary>
    /// Get the cell background color for a given status and month context.
    /// </summary>
    public static string GetCellBackgroundColor(string status, bool isCurrentMonth)
    {
        return (status, isCurrentMonth) switch
        {
            (StatusShipped, true) => ColorShippedCellCurrentMonth,
            (StatusShipped, false) => ColorShippedCellDefault,
            (StatusInProgress, true) => ColorInProgressCellCurrentMonth,
            (StatusInProgress, false) => ColorInProgressCellDefault,
            (StatusCarryover, true) => ColorCarryoverCellCurrentMonth,
            (StatusCarryover, false) => ColorCarryoverCellDefault,
            (StatusBlockers, true) => ColorBlockersCellCurrentMonth,
            (StatusBlockers, false) => ColorBlockersCellDefault,
            _ => ColorPageBackground
        };
    }

    /// <summary>
    /// Get the CSS class name for a heatmap cell.
    /// </summary>
    public static string GetCellCssClass(string status, bool isCurrentMonth)
    {
        var baseClass = status switch
        {
            StatusShipped => CssClassShippedCell,
            StatusInProgress => CssClassInProgressCell,
            StatusCarryover => CssClassCarryoverCell,
            StatusBlockers => CssClassBlockersCell,
            _ => string.Empty
        };

        if (isCurrentMonth && !string.IsNullOrEmpty(baseClass))
        {
            return $"{baseClass} {CssClassCurrentMonth}";
        }

        return baseClass;
    }

    /// <summary>
    /// Get the row header CSS class name for a given status.
    /// </summary>
    public static string GetRowHeaderCssClass(string status)
    {
        return status switch
        {
            StatusShipped => CssClassShippedHeaderRow,
            StatusInProgress => CssClassInProgressHeaderRow,
            StatusCarryover => CssClassCarryoverHeaderRow,
            StatusBlockers => CssClassBlockersHeaderRow,
            _ => string.Empty
        };
    }

    /// <summary>
    /// Get the status dot color for a given status.
    /// </summary>
    public static string GetStatusDotColor(string status)
    {
        return status switch
        {
            StatusShipped => ColorShippedDot,
            StatusInProgress => ColorInProgressDot,
            StatusCarryover => ColorCarryoverDot,
            StatusBlockers => ColorBlockersDot,
            _ => ColorSecondaryText
        };
    }

    /// <summary>
    /// Get the row header background color for a given status.
    /// </summary>
    public static string GetRowHeaderBackgroundColor(string status)
    {
        return status switch
        {
            StatusShipped => ColorShippedHeaderBackground,
            StatusInProgress => ColorInProgressHeaderBackground,
            StatusCarryover => ColorCarryoverHeaderBackground,
            StatusBlockers => ColorBlockersHeaderBackground,
            _ => ColorHeaderBackground
        };
    }

    /// <summary>
    /// Get the row header text color for a given status.
    /// </summary>
    public static string GetRowHeaderTextColor(string status)
    {
        return status switch
        {
            StatusShipped => ColorShippedHeaderText,
            StatusInProgress => ColorInProgressHeaderText,
            StatusCarryover => ColorCarryoverHeaderText,
            StatusBlockers => ColorBlockersHeaderText,
            _ => ColorPrimaryText
        };
    }

    /// <summary>
    /// Get the color for a milestone type.
    /// </summary>
    public static string GetMilestoneColor(string milestoneType)
    {
        return milestoneType switch
        {
            MilestoneTypePoC => ColorMilestonePoCDiamond,
            MilestoneTypeRelease => ColorMilestoneProductionDiamond,
            MilestoneTypeCheckpoint => ColorMilestoneCheckpointCircle,
            _ => ColorSecondaryText
        };
    }
}