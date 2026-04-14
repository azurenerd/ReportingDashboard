using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData
{
    [JsonPropertyName("schemaVersion")]
    public required int SchemaVersion { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("subtitle")]
    public required string Subtitle { get; init; }

    [JsonPropertyName("backlogUrl")]
    public required string BacklogUrl { get; init; }

    [JsonPropertyName("timeline")]
    public required TimelineConfig Timeline { get; init; }

    [JsonPropertyName("heatmap")]
    public required HeatmapConfig Heatmap { get; init; }

    /// <summary>
    /// ISO date string (e.g. "2026-04-14") to override the NOW line position.
    /// When null, the system date is used.
    /// </summary>
    [JsonPropertyName("nowDateOverride")]
    public string? NowDateOverride { get; init; }

    /// <summary>
    /// Abbreviated month name (e.g. "Apr") to override current-month highlighting.
    /// When null, derived from the effective date (nowDateOverride or system date).
    /// </summary>
    [JsonPropertyName("currentMonthOverride")]
    public string? CurrentMonthOverride { get; init; }
}

public record TimelineConfig
{
    [JsonPropertyName("startDate")]
    public required string StartDate { get; init; }

    [JsonPropertyName("endDate")]
    public required string EndDate { get; init; }

    [JsonPropertyName("workstreams")]
    public required Workstream[] Workstreams { get; init; }
}

public record Workstream
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("color")]
    public required string Color { get; init; }

    [JsonPropertyName("milestones")]
    public required Milestone[] Milestones { get; init; }
}

public record Milestone
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("date")]
    public required string Date { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("labelPosition")]
    public string? LabelPosition { get; init; }
}

public record HeatmapConfig
{
    [JsonPropertyName("monthColumns")]
    public required string[] MonthColumns { get; init; }

    [JsonPropertyName("categories")]
    public required StatusCategory[] Categories { get; init; }
}

public record StatusCategory
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("emoji")]
    public required string Emoji { get; init; }

    [JsonPropertyName("cssClass")]
    public required string CssClass { get; init; }

    [JsonPropertyName("months")]
    public required MonthItems[] Months { get; init; }
}

public record MonthItems
{
    [JsonPropertyName("month")]
    public required string Month { get; init; }

    [JsonPropertyName("items")]
    public required string[] Items { get; init; }
}