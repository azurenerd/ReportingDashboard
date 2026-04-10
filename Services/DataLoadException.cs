namespace AgentSquad.Runner.Services;

/// <summary>
/// Base exception for data loading errors.
/// </summary>
public class DataLoadException : Exception
{
    public DataLoadException(string message) : base(message)
    {
    }

    public DataLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}