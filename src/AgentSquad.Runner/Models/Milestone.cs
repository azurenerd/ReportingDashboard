namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a single milestone on the project timeline.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Unique identifier for the milestone.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display label/name of the milestone (e.g., "PoC Launch", "Production Release").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Planned delivery date for this milestone in ISO 8601 format (YYYY-MM-DD).
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Type of milestone: "poc", "release", or "checkpoint".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the milestone.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// SVG x-coordinate position for rendering (calculated by DateCalculationService).
    /// </summary>
    public int? XPosition { get; set; }
}