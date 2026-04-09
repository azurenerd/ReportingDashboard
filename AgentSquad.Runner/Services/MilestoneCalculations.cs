using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public static class MilestoneCalculations
{
    /// <summary>
    /// Calculates the number of days remaining until a milestone target date.
    /// </summary>
    /// <param name="targetDate">Target date in UTC</param>
    /// <returns>
    /// Positive integer: days remaining until target date
    /// Zero or negative: milestone is overdue (days past target date)
    /// </returns>
    /// <remarks>
    /// Uses DateTime.UtcNow for consistency; all dates assumed UTC in data.json.
    /// Calculation: (targetDate - DateTime.UtcNow).TotalDays rounded to integer.
    /// </remarks>
    public static int CalculateDaysRemaining(DateTime targetDate)
    {
        var daysRemaining = (targetDate - DateTime.UtcNow).TotalDays;
        return (int)Math.Round(daysRemaining, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Determines if a milestone is overdue (>= 3 days past target date).
    /// </summary>
    /// <param name="targetDate">Target date in UTC</param>
    /// <returns>True if milestone is 3 or more days overdue; false otherwise</returns>
    /// <remarks>
    /// Threshold: 3 days overdue triggers "At Risk" status in UI.
    /// Milestones 0-2 days overdue are not flagged as at risk.
    /// </remarks>
    public static bool IsOverdue(DateTime targetDate)
    {
        var daysRemaining = CalculateDaysRemaining(targetDate);
        return daysRemaining <= -3;
    }

    /// <summary>
    /// Computes a user-friendly status label for a milestone.
    /// </summary>
    /// <param name="status">Milestone status enum (Planned, InProgress, Completed, AtRisk)</param>
    /// <param name="daysRemaining">Days remaining until target date (from CalculateDaysRemaining)</param>
    /// <returns>Human-readable status label for UI display</returns>
    /// <remarks>
    /// Label logic:
    /// - Completed: "Completed"
    /// - AtRisk: "At Risk"
    /// - InProgress (daysRemaining > 0): "In Progress" + days remaining info
    /// - InProgress (daysRemaining <= 0): "At Risk - Overdue"
    /// - Planned (daysRemaining > 7): "Planned"
    /// - Planned (daysRemaining <= 7 and > 0): "Planned - Approaching"
    /// - Planned (daysRemaining <= 0): "At Risk - Overdue"
    /// </remarks>
    public static string ComputeStatusLabel(MilestoneStatus status, int daysRemaining)
    {
        return status switch
        {
            MilestoneStatus.Completed => "Completed",
            MilestoneStatus.AtRisk => "At Risk",
            MilestoneStatus.InProgress when daysRemaining > 0 => $"In Progress ({daysRemaining}d)",
            MilestoneStatus.InProgress => "At Risk - Overdue",
            MilestoneStatus.Planned when daysRemaining > 7 => "Planned",
            MilestoneStatus.Planned when daysRemaining > 0 => $"Planned - {daysRemaining}d",
            MilestoneStatus.Planned => "At Risk - Overdue",
            _ => "Unknown"
        };
    }
}