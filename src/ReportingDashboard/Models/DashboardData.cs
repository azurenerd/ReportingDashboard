using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class DashboardData
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = "";

    [JsonPropertyName("backlogUrl")]
    public string BacklogUrl { get; set; } = "";

    [JsonPropertyName("timeline")]
    public TimelineConfig Timeline { get; set; } = new();

    [JsonPropertyName("heatmap")]
    public HeatmapConfig Heatmap { get; set; } = new();

    [JsonPropertyName("nowDateOverride")]
    public string? NowDateOverride { get; set; }

    [JsonPropertyName("currentMonthOverride")]
    public string? CurrentMonthOverride { get; set; }
}

public class TimelineConfig
{
    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("workstreams")]
    public Workstream[] Workstreams { get; set; } = Array.Empty<Workstream>();
}

public class Workstream
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("milestones")]
    public Milestone[] Milestones { get; set; } = Array.Empty<Milestone>();
}

public class Milestone
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("labelPosition")]
    public string? LabelPosition { get; set; }
}

public class HeatmapConfig
{
    [JsonPropertyName("monthColumns")]
    public string[] MonthColumns { get; set; } = Array.Empty<string>();

    [JsonPropertyName("categories")]
    public StatusCategory[] Categories { get; set; } = Array.Empty<StatusCategory>();
}

public class StatusCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("emoji")]
    public string Emoji { get; set; } = "";

    [JsonPropertyName("cssClass")]
    public string CssClass { get; set; } = "";

    [JsonPropertyName("months")]
    public MonthItems[] Months { get; set; } = Array.Empty<MonthItems>();
}

public class MonthItems
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = "";

    [JsonPropertyName("items")]
    public string[] Items { get; set; } = Array.Empty<string>();
}