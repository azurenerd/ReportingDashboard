namespace ReportingDashboard.Models;

using System.Text.Json.Serialization;

public record DashboardData(
    ProjectInfo Project,
    TimelineConfig Timeline,
    List<MilestoneTrack> Tracks,
    HeatmapData Heatmap
);

public record ProjectInfo(
    string Title,
    string Subtitle,
    string BacklogUrl,
    string CurrentMonth)
{
    [JsonPropertyName("legend")]
    public List<LegendItem>? Legend { get; init; }
}

public record LegendItem(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("color")] string Color
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
    [property: JsonPropertyName("label")] string? Label = null
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