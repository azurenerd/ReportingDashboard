namespace AgentSquad.Runner.Data.Exceptions;

/// <summary>
/// Custom exception thrown when project data fails to load, validate, or deserialize.
/// </summary>
public class DataLoadException : Exception
{
    /// <summary>
    /// Gets the error message describing the data load failure.
    /// </summary>
    public override string Message { get; }

    /// <summary>
    /// Initializes a new instance of the DataLoadException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that describes the data load failure.</param>
    public DataLoadException(string message) : base(message)
    {
        Message = message;
    }

    /// <summary>
    /// Initializes a new instance of the DataLoadException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that describes the data load failure.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DataLoadException(string message, Exception innerException) : base(message, innerException)
    {
        Message = message;
    }
}