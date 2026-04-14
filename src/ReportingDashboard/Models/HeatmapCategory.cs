using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class HeatmapCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; set; } = new();
}