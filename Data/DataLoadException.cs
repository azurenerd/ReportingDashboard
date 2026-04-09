namespace AgentSquad.Runner.Data
{
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