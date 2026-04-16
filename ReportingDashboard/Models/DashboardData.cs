using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardData
{
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; init; }

    [JsonPropertyName("backlogUrl")]
    public string? BacklogUrl { get; init; }

    [JsonPropertyName("timelineMonths")]
    public string[]? TimelineMonths { get; init; }

    [JsonPropertyName("currentMonth")]
    public string? CurrentMonth { get; init; }

    [JsonPropertyName("milestones")]
    public MilestoneTrack[]? Milestones { get; init; }

    [JsonPropertyName("heatmap")]
    public HeatmapData? Heatmap { get; init; }
}

public record MilestoneTrack
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = "";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    [JsonPropertyName("color")]
    public string Color { get; init; } = "#000000";

    [JsonPropertyName("events")]
    public MilestoneEvent[] Events { get; init; } = Array.Empty<MilestoneEvent>();
}

public record MilestoneEvent
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = "checkpoint";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";
}

public record HeatmapData
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; init; }

    [JsonPropertyName("rows")]
    public HeatmapRow[] Rows { get; init; }

    public HeatmapData() : this(Array.Empty<string>(), Array.Empty<HeatmapRow>()) { }

    [JsonConstructor]
    public HeatmapData(string[] columns, HeatmapRow[] rows)
    {
        Columns = columns;
        Rows = rows;
    }
}

public record HeatmapRow
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = "";

    [JsonPropertyName("items")]
    public Dictionary<string, string[]> Items { get; init; } = new();
}

public record CategoryStyle(
    string HeaderText,
    string HeaderBg,
    string CellBg,
    string CellAccent,
    string Bullet,
    string DisplayName
)
{
    public static CategoryStyle GetCategoryStyle(string category) => category switch
    {
        "shipped" => new CategoryStyle("#1B7A28", "#E8F5E9", "#F0FBF0", "#D8F2DA", "#34A853", "Shipped"),
        "in-progress" => new CategoryStyle("#1565C0", "#E3F2FD", "#EEF4FE", "#DAE8FB", "#0078D4", "In Progress"),
        "carryover" => new CategoryStyle("#B45309", "#FFF8E1", "#FFFDE7", "#FFF0B0", "#F4B400", "Carryover"),
        "blockers" => new CategoryStyle("#991B1B", "#FEF2F2", "#FFF5F5", "#FFE4E4", "#EA4335", "Blockers"),
        _ => new CategoryStyle("#666", "#F5F5F5", "#FAFAFA", "#F0F0F0", "#999", category)
    };
}