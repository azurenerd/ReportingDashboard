namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a single month in the dashboard heatmap display.
/// </summary>
public class MonthInfo
{
    /// <summary>
    /// Month name (e.g., "January", "February").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Year of the month.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Start date of the month.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the month.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Zero-based column index in the heatmap grid (0-3 for 4-month display).
    /// </summary>
    public int GridColumnIndex { get; set; }

    /// <summary>
    /// True if this month contains the current date.
    /// </summary>
    public bool IsCurrentMonth { get; set; }
}