namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a single validation error for a specific field in a ProjectReport.
/// Contains the field path and a user-friendly error message.
/// Immutable data container; no validation logic.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Gets or sets the field path that failed validation.
    /// Examples: "projectName", "milestones[0].status", "statusSnapshot.shipped[1]"
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the user-friendly error message describing the validation failure.
    /// Guides the user on how to fix the issue.
    /// Examples: "Project name is required and cannot be empty."
    ///           "Milestone 'm1': Invalid status 'in_progress'. Must be one of: on-track, at-risk, delayed, completed."
    /// </summary>
    public string Message { get; set; }
}