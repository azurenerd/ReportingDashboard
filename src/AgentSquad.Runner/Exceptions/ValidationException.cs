using System;

namespace AgentSquad.Runner.Exceptions
{
    /// <summary>
    /// Exception thrown when dashboard data fails validation
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}