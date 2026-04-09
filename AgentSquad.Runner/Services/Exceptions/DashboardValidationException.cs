namespace AgentSquad.Runner.Services.Exceptions;

/// <summary>
/// Exception thrown when dashboard data fails validation.
/// </summary>
/// <remarks>
/// Contains list of validation errors for user feedback.
/// Used by DashboardService.ValidateDataAsync to report specific violations.
/// </remarks>
public class DashboardValidationException : Exception
{
    /// <summary>
    /// List of validation error messages (not shown to users, logged only).
    /// </summary>
    public List<string> Errors { get; set; } = new();

    public DashboardValidationException(string message) : base(message)
    {
    }

    public DashboardValidationException(string message, List<string> errors)
        : base(message)
    {
        Errors = errors ?? new List<string>();
    }

    public DashboardValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DashboardValidationException(string message, List<string> errors, Exception innerException)
        : base(message, innerException)
    {
        Errors = errors ?? new List<string>();
    }
}