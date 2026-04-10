namespace AgentSquad.Runner.Services;

/// <summary>
/// Exception thrown when file I/O operations fail.
/// </summary>
public class FileReadException : DataLoadException
{
    public FileReadException(string message) : base(message)
    {
    }

    public FileReadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}