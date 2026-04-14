using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class HeatmapData
{
    [JsonPropertyName("shipped")]
    public Dictionary<string, List<string>> Shipped { get; set; } = new Dictionary<string, List<string>>();

    [JsonPropertyName("inProgress")]
    public Dictionary<string, List<string>> InProgress { get; set; } = new Dictionary<string, List<string>>();

    [JsonPropertyName("carryover")]
    public Dictionary<string, List<string>> Carryover { get; set; } = new Dictionary<string, List<string>>();

    [JsonPropertyName("blockers")]
    public Dictionary<string, List<string>> Blockers { get; set; } = new Dictionary<string, List<string>>();
}