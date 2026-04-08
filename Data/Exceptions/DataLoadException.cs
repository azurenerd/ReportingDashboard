using System;

namespace AgentSquad.Runner.Data.Exceptions
{
    /// <summary>
    /// Exception thrown when project data loading or parsing fails.
    /// </summary>
    /// <remarks>
    /// This custom exception is used to wrap and normalize errors that occur during JSON deserialization,
    /// file I/O operations, and data validation in the ProjectDataService. It ensures that all data-related
    /// errors are presented to users with friendly, non-technical messages that do not expose internal
    /// implementation details or file paths.
    /// </remarks>
    public class DataLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataLoadException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public DataLoadException(string message)
            : base(message)
        {
        }
    }
}