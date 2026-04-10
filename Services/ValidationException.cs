namespace AgentSquad.Runner.Services;

/// <summary>
/// Exception thrown when data validation fails.
/// </summary>
public class ValidationException : DataLoadException
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}