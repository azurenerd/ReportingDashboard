namespace AgentSquad.Runner.Services;

/// <summary>
/// Exception thrown when JSON deserialization fails.
/// </summary>
public class JsonParseException : DataLoadException
{
    public JsonParseException(string message) : base(message)
    {
    }

    public JsonParseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}