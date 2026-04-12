#nullable enable

using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a timeline milestone event (PoC, production release, checkpoint).
/// Milestones are positioned on the SVG timeline based on their date and type.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Unique identifier for the milestone within the milestones array.
    /// Required, must be unique.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the milestone (e.g., "Chatbot & MS Role", "Production Release").
    /// Required, max 100 characters, must be non-empty.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Milestone date in ISO 8601 format (YYYY-MM-DD).
    /// Required, must be a valid date.
    /// </summary>
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Milestone type: "poc", "release", or "checkpoint".
    /// Determines how the milestone is visualized on the timeline (diamond vs. circle).
    /// Required, must be one of the valid types.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}