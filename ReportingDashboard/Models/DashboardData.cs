using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("subtitle")] string Subtitle,
    [property: JsonPropertyName("backlogUrl")] string BacklogUrl,
    [property: JsonPropertyName("currentDate")] string CurrentDate,
    [property: JsonPropertyName("timelineStart")] string TimelineStart,
    [property: JsonPropertyName("timelineEnd")] string TimelineEnd,
    [property: JsonPropertyName("milestoneStreams")] List<MilestoneStream> MilestoneStreams,
    [property: JsonPropertyName("heatmap")] HeatmapData Heatmap
);

public record MilestoneStream(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("milestones")] List<Milestone> Milestones
);

public record Milestone(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("labelPosition")] string? LabelPosition = null
);

public record HeatmapData(
    [property: JsonPropertyName("columns")] List<string> Columns,
    [property: JsonPropertyName("currentColumnIndex")] int CurrentColumnIndex,
    [property: JsonPropertyName("rows")] List<HeatmapRow> Rows
);

public record HeatmapRow(
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("items")] Dictionary<string, List<string>> Items
);