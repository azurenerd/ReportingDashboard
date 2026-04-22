using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

/// <summary>
/// Heatmap section: months, highlight, and status rows.
/// </summary>
public record HeatmapData
{
    [JsonPropertyName("months")]
    public List<string> Months { get; init; } = new();

    [JsonPropertyName("highlightMonth")]
    public string HighlightMonth { get; init; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<StatusRow> Rows { get; init; } = new();
}

/// <summary>
/// A single status category row (Shipped, In Progress, Carryover, Blockers).
/// </summary>
public record StatusRow
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; init; } = new();
}