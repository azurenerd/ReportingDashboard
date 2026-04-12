using System;

namespace AgentSquad.Runner.Exceptions
{
    /// <summary>
    /// Exception thrown when dashboard data cannot be loaded or deserialized
    /// </summary>
    public class DashboardDataException : Exception
    {
        public DashboardDataException(string message) : base(message)
        {
        }

        public DashboardDataException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}