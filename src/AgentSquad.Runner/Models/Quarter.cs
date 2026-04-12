#nullable enable

using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a quarter (month) of project status data.
/// Contains lists of items in each status category (shipped, in progress, carryover, blockers).
/// </summary>
public class Quarter
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("shipped")]
    public List<string> Shipped { get; set; } = new();

    [JsonPropertyName("inProgress")]
    public List<string> InProgress { get; set; } = new();

    [JsonPropertyName("carryover")]
    public List<string> Carryover { get; set; } = new();

    [JsonPropertyName("blockers")]
    public List<string> Blockers { get; set; } = new();
}