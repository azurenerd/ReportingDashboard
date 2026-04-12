using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Root configuration object deserialized from data.json.
/// Represents the complete dashboard configuration including project metadata, quarterly status data, and milestones.
/// </summary>
public class DashboardConfig
{
    /// <summary>
    /// Project name displayed in the dashboard header.
    /// Required, max 255 characters.
    /// </summary>
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Project description displayed as subtitle in the dashboard header.
    /// Format: "[Organization]  [Workstream]  [Month Year]"
    /// Required, max 255 characters.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Array of quarterly status data, one per month.
    /// Each quarter contains shipped items, in-progress work, carryover items, and blockers.
    /// Required, 1-12 items.
    /// </summary>
    [JsonPropertyName("quarters")]
    public List<Quarter> Quarters { get; set; } = new();

    /// <summary>
    /// Array of timeline milestones (PoC, production releases, checkpoints).
    /// Each milestone has an ID, label, date, and type.
    /// Optional, 0-50 items.
    /// </summary>
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}