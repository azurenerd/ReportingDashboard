namespace ReportingDashboard.Models;

/// <summary>
/// Root model for the dashboard data.json file.
/// All dashboard content is driven from this structure.
/// </summary>
public class DashboardData
{
    /// <summary>Project title displayed in the header (24px bold).</summary>
    public string Title { get; set; } = "";

    /// <summary>Subtitle line: org, workstream, and month (12px gray).</summary>
    public string Subtitle { get; set; } = "";

    /// <summary>URL for the ADO Backlog link. If null/empty, link is hidden.</summary>
    public string? BacklogUrl { get; set; }

    /// <summary>
    /// Controls the NOW line position on the timeline.
    /// Format: "yyyy-MM-dd". Uses this instead of system date for reproducible screenshots.
    /// </summary>
    public string CurrentDate { get; set; } = "";

    /// <summary>Timeline/Gantt chart configuration.</summary>
    public TimelineConfig Timeline { get; set; } = new();

    /// <summary>Monthly execution heatmap configuration.</summary>
    public HeatmapConfig Heatmap { get; set; } = new();
}

/// <summary>Configuration for the SVG timeline section.</summary>
public class TimelineConfig
{
    /// <summary>Timeline start date (left edge). Format: "yyyy-MM-dd".</summary>
    public string StartDate { get; set; } = "";

    /// <summary>Timeline end date (right edge). Format: "yyyy-MM-dd".</summary>
    public string EndDate { get; set; } = "";

    /// <summary>Milestone tracks (2-5 supported without overflow).</summary>
    public List<TrackConfig> Tracks { get; set; } = new();
}

/// <summary>A single milestone track (horizontal line with markers).</summary>
public class TrackConfig
{
    /// <summary>Track identifier displayed in the sidebar (e.g., "M1").</summary>
    public string Id { get; set; } = "";

    /// <summary>Track description displayed below the ID.</summary>
    public string Label { get; set; } = "";

    /// <summary>Track color as CSS hex (e.g., "#0078D4").</summary>
    public string Color { get; set; } = "#999";

    /// <summary>Milestones positioned on this track.</summary>
    public List<MilestoneConfig> Milestones { get; set; } = new();
}

/// <summary>A milestone marker on a timeline track.</summary>
public class MilestoneConfig
{
    /// <summary>Milestone date. Format: "yyyy-MM-dd".</summary>
    public string Date { get; set; } = "";

    /// <summary>
    /// Milestone type determines the shape:
    /// "checkpoint" = circle, "poc" = gold diamond, "production" = green diamond.
    /// </summary>
    public string Type { get; set; } = "checkpoint";

    /// <summary>Label displayed near the milestone marker.</summary>
    public string Label { get; set; } = "";
}

/// <summary>Configuration for the monthly execution heatmap.</summary>
public class HeatmapConfig
{
    /// <summary>Month column headers in display order (e.g., ["January", "February", ...]).</summary>
    public List<string> Months { get; set; } = new();

    /// <summary>Which month is "current" - gets highlighted styling.</summary>
    public string CurrentMonth { get; set; } = "";

    /// <summary>Status rows. Expected: shipped, inprogress, carryover, blockers.</summary>
    public List<HeatmapRow> Rows { get; set; } = new();
}

/// <summary>A single status row in the heatmap (e.g., "shipped").</summary>
public class HeatmapRow
{
    /// <summary>
    /// Status key: "shipped", "inprogress", "carryover", or "blockers".
    /// Maps to CSS class prefixes and row header labels.
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// Work items keyed by month name. Each month maps to a list of short descriptions.
    /// Missing months or empty arrays render as a dash in the cell.
    /// </summary>
    public Dictionary<string, List<string>> Items { get; set; } = new();
}