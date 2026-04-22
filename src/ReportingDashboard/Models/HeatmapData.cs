using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record HeatmapData
{
    [JsonPropertyName("months")]
    public List<string> Months { get; init; } = new();

    [JsonPropertyName("highlightMonth")]
    public string HighlightMonth { get; init; } = "";

    [JsonPropertyName("rows")]
    public List<StatusRow> Rows { get; init; } = new();
}

public record StatusRow
{
    [JsonPropertyName("category")]
    public string Category { get; init; } = "";

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; init; } = new();
}