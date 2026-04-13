using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class WorkItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("category")]
    public string Category { get; set; } = "";

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = "";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "";

    [JsonPropertyName("percentComplete")]
    public int PercentComplete { get; set; }

    [JsonPropertyName("carryOverReason")]
    public string CarryOverReason { get; set; } = "";

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = "";
}