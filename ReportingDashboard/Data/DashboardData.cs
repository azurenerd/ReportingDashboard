using System.Text.Json.Serialization;

namespace ReportingDashboard.Data;

public class DashboardData
{
    public required ProjectInfo Project { get; set; }
    public required TimelineConfig Timeline { get; set; }
    public required HeatmapData Heatmap { get; set; }
}

public class ProjectInfo
{
    public required string Title { get; set; }
    public required string Subtitle { get; set; }
    public required string BacklogUrl { get; set; }
    public string? BacklogLinkText { get; set; }
}

public class TimelineConfig
{
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    public DateTime? NowDate { get; set; }
    public required List<TimelineTrack> Tracks { get; set; }
}

public class TimelineTrack
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required List<TrackMilestone> Milestones { get; set; }
}

public class TrackMilestone
{
    public required DateTime Date { get; set; }
    public required string Type { get; set; }
    public string? Label { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LabelPosition LabelPosition { get; set; } = LabelPosition.Above;
}

public enum LabelPosition
{
    Above,
    Below
}

public class HeatmapData
{
    public required List<string> Columns { get; set; }
    public required string CurrentColumn { get; set; }
    public required List<HeatmapRow> Rows { get; set; }
}

public class HeatmapRow
{
    public required string Category { get; set; }
    public required Dictionary<string, List<string>> Items { get; set; }
}