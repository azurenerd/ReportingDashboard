using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData(
    [property: JsonPropertyName("project")] ProjectInfo Project,
    [property: JsonPropertyName("timeline")] TimelineConfig Timeline,
    [property: JsonPropertyName("tracks")] List<MilestoneTrack> Tracks,
    [property: JsonPropertyName("heatmap")] HeatmapData Heatmap
);

public record ProjectInfo(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("subtitle")] string Subtitle,
    [property: JsonPropertyName("backlogUrl")] string BacklogUrl,
    [property: JsonPropertyName("currentMonth")] string CurrentMonth
);

public record TimelineConfig(
    [property: JsonPropertyName("months")] List<string> Months,
    [property: JsonPropertyName("nowPosition")] double NowPosition
);

public record MilestoneTrack(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("color")] string Color,
    [property: JsonPropertyName("milestones")] List<Milestone> Milestones
);

public record Milestone(
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("position")] double Position,
    [property: JsonPropertyName("label")] string? Label
);

public record HeatmapData(
    [property: JsonPropertyName("months")] List<string> Months,
    [property: JsonPropertyName("categories")] List<HeatmapCategory> Categories
);

public record HeatmapCategory(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cssClass")] string CssClass,
    [property: JsonPropertyName("emoji")] string Emoji,
    [property: JsonPropertyName("items")] Dictionary<string, List<string>> Items
);