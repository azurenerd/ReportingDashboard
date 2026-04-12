#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents project status data for a single month.
/// Contains lists of shipped items, in-progress work, carryover items, and blockers.
/// </summary>
public class Quarter
{
    /// <summary>
    /// Month name (e.g., "January", "February", etc.).
    /// Required, must be a valid month name (January-December).
    /// </summary>
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Year (e.g., 2026).
    /// Required, should be in range 2000-2099.
    /// </summary>
    [JsonPropertyName("year")]
    public int Year { get; set; }

    /// <summary>
    /// List of items shipped/completed in this month.
    /// Optional, 0-50 items.
    /// </summary>
    [JsonPropertyName("shipped")]
    public List<string> Shipped { get; set; } = new();

    /// <summary>
    /// List of items currently in progress during this month.
    /// Optional, 0-50 items.
    /// </summary>
    [JsonPropertyName("inProgress")]
    public List<string> InProgress { get; set; } = new();

    /// <summary>
    /// List of items carried over from previous month(s).
    /// Optional, 0-50 items.
    /// </summary>
    [JsonPropertyName("carryover")]
    public List<string> Carryover { get; set; } = new();

    /// <summary>
    /// List of items blocked by dependencies or issues.
    /// Optional, 0-50 items.
    /// </summary>
    [JsonPropertyName("blockers")]
    public List<string> Blockers { get; set; } = new();
}