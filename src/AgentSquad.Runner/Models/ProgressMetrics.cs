namespace AgentSquad.Runner.Models;

/// <summary>
/// Calculated aggregate metrics displayed in the metrics footer.
/// All metrics are computed by ProjectDataService during JSON deserialization
/// based on work item counts and are never manually set from JSON.
/// </summary>
public class ProgressMetrics
{
    /// <summary>
    /// Total planned items: Shipped.Count + InProgress.Count + CarriedOver.Count
    /// Represents the full scope of tracked work.
    /// </summary>
    public int TotalPlanned { get; set; }

    /// <summary>
    /// Completed items: Shipped.Count
    /// Represents work successfully delivered.
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// In-flight items: InProgress.Count + CarriedOver.Count
    /// Represents active and deferred work remaining.
    /// </summary>
    public int InFlight { get; set; }

    /// <summary>
    /// Health score percentage: (Completed / TotalPlanned) * 100
    /// Range: 0-100 (decimal)
    /// Used to color-code the metrics footer:
    /// - >= 75%: green (healthy)
    /// - >= 50%: orange (at-risk)
    /// - < 50%: red (blocked/critical)
    /// Handles division-by-zero: defaults to 0 if TotalPlanned is 0.
    /// </summary>
    public decimal HealthScore { get; set; }
}