namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents the result of validating a ProjectReport against the schema.
/// Contains a boolean validity flag and a collection of validation errors (if any).
/// Immutable data container; no validation logic.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the validation passed.
    /// true if all schema rules satisfied; false otherwise.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the collection of validation errors.
    /// Empty if IsValid is true; populated with specific field errors if IsValid is false.
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();
}