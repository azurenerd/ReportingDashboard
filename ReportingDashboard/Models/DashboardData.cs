using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("subtitle")] string Subtitle,
    [property: JsonPropertyName("backlogUrl")] string BacklogUrl,
    [property: JsonPropertyName("reportDate")] string ReportDate,
    [property: JsonPropertyName("timeline")] TimelineRange Timeline,
    [property: JsonPropertyName("workstreams")] List<Workstream> Workstreams,
    [property: JsonPropertyName("heatmap")] HeatmapData Heatmap
);

public record TimelineRange(
    [property: JsonPropertyName("startDate")] string StartDate,
    [property: JsonPropertyName("endDate")] string EndDate,
    [property: JsonPropertyName("months")] List<MonthGridline> Months
);

public record MonthGridline(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("date")] string Date
);

public record Workstream(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("label")] string Label,
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
    [property: JsonPropertyName("months")] List<string> Months,
    [property: JsonPropertyName("currentMonth")] string CurrentMonth,
    [property: JsonPropertyName("rows")] List<HeatmapRow> Rows
);

public record HeatmapRow(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("displayLabel")] string DisplayLabel,
    [property: JsonPropertyName("cells")] List<HeatmapCell> Cells
);

public record HeatmapCell(
    [property: JsonPropertyName("month")] string Month,
    [property: JsonPropertyName("items")] List<string> Items
);