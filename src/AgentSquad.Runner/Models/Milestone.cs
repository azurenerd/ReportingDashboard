using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a timeline milestone (PoC, production release, or checkpoint)
/// </summary>
public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty; // ISO 8601 format: "2026-04-30"

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "poc" | "release" | "checkpoint"
}