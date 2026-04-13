using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class Milestone
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#0078D4";

    [JsonPropertyName("events")]
    public List<MilestoneEvent> Events { get; set; } = new();
}

public class MilestoneEvent
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public MilestoneEventType Type { get; set; } = MilestoneEventType.Checkpoint;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneEventType
{
    Checkpoint,
    PoC,
    Production
}