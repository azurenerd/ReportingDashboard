namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Custom exception thrown when project data loading, parsing, or validation fails.
    /// Used to distinguish data loading errors from other application exceptions.
    /// </summary>
    public class DataLoadException : Exception
    {
        public DataLoadException(string message) : base(message)
        {
        }

        public DataLoadException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}