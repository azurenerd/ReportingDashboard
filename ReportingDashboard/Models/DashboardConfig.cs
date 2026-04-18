using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardConfig
{
    public ProjectConfig Project { get; set; } = new();
    public TimelineConfig Timeline { get; set; } = new();
    public HeatmapConfig Heatmap { get; set; } = new();
}

public class ProjectConfig
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? AdoLink { get; set; }
}

public class TimelineConfig
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? NowDate { get; set; }
    public List<MilestoneTrack> Milestones { get; set; } = new();
}

public class MilestoneTrack
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#888";
    public List<MilestoneEvent> Events { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneEventType { Checkpoint, Poc, Release }

public class MilestoneEvent
{
    public DateOnly Date { get; set; }
    public MilestoneEventType Type { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class HeatmapConfig
{
    public List<string> Columns { get; set; } = new();
    public string? CurrentColumn { get; set; }
    public List<HeatmapRow> Rows { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HeatmapRowType { Shipped, InProgress, Carryover, Blocker }

public class HeatmapRow
{
    public HeatmapRowType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<HeatmapCell> Cells { get; set; } = new();
}

public class HeatmapCell
{
    public string Month { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new();
}