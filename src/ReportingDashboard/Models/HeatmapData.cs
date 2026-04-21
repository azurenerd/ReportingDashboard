using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class HeatmapData
{
    [JsonPropertyName("months")]
    public List<string> Months { get; set; } = new();

    [JsonPropertyName("currentMonth")]
    public string CurrentMonth { get; set; } = "";

    [JsonPropertyName("categories")]
    public List<HeatmapCategory> Categories { get; set; } = new();
}

public class HeatmapCategory
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("colorClass")]
    public string ColorClass { get; set; } = "";

    [JsonPropertyName("items")]
    public Dictionary<string, List<string>> Items { get; set; } = new();
}