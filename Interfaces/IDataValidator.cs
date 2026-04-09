namespace AgentSquad.Runner.Interfaces;

using AgentSquad.Runner.Models;

/// <summary>
/// Service contract for validating ProjectReport schema and structure.
/// 
/// Responsibility: Validate ProjectReport schema. Check required fields, enum values, 
/// data types, ranges. Immutable validation logic.
/// 
/// Lifetime: Singleton (stateless, thread-safe)
/// </summary>
public interface IDataValidator
{
    /// <summary>
    /// Validate a ProjectReport object against the schema rules.
    /// </summary>
    /// <param name="report">
    /// The ProjectReport object to validate. Can be null (validation fails with specific error).
    /// </param>
    /// <returns>
    /// ValidationResult object containing:
    /// - IsValid: true if all schema rules satisfied, false otherwise
    /// - Errors: empty list if valid; populated with field-specific errors if invalid
    /// </returns>
    /// <remarks>
    /// Does NOT throw exceptions. Always returns ValidationResult with errors collection.
    /// 
    /// Validation Rules:
    /// - projectName: required, non-empty, max 100 chars
    /// - reportingPeriod: required, non-empty, max 50 chars
    /// - milestones: required array, min length 1
    ///   - id: required, unique within array, max 50 chars
    ///   - name: required, non-empty, max 100 chars
    ///   - targetDate: required, valid ISO 8601 (YYYY-MM-DD)
    ///   - status: required, one of ["on-track", "at-risk", "delayed", "completed"] (lowercase)
    ///   - progress: required, integer 0-100 inclusive
    ///   - description: optional, max 500 chars
    /// - statusSnapshot: required object
    ///   - shipped: required array (can be empty), each item max 200 chars
    ///   - inProgress: required array (can be empty), each item max 200 chars
    ///   - carriedOver: required array (can be empty), each item max 200 chars
    /// - kpis: optional dictionary, each key max 50 chars, each value 0-100
    /// 
    /// Implementation Details:
    /// - Create ValidationResult with IsValid=true initially
    /// - Check each field, accumulate errors (do not fail-fast)
    /// - Return ValidationResult with IsValid flag and all Errors found
    /// - Log warnings if validation fails (INFO level)
    /// 
    /// Error Messages (User-Facing):
    /// "Project name is required and cannot be empty."
    /// "Reporting period is required."
    /// "At least one milestone is required."
    /// "Milestone '{id}': Invalid target date format. Use YYYY-MM-DD (e.g., 2026-05-15)."
    /// "Milestone '{id}': Invalid status '{value}'. Must be one of: on-track, at-risk, delayed, completed."
    /// "Milestone '{id}': Progress must be a number between 0 and 100."
    /// "KPI '{key}': Value must be a number between 0 and 100."
    /// </remarks>
    ValidationResult Validate(ProjectReport report);
}