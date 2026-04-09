namespace AgentSquad.Runner.Services.Exceptions;

/// <summary>
/// Exception thrown when dashboard data cannot be loaded from data.json.
/// </summary>
/// <remarks>
/// Wraps file I/O errors, JSON parsing errors, and validation failures.
/// Used by DashboardService.LoadDataAsync to provide consistent error handling.
/// All instances include user-friendly message (no stack traces exposed to UI).
/// </remarks>
public class DashboardLoadException : Exception
{
    public DashboardLoadException(string message) : base(message)
    {
    }

    public DashboardLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}