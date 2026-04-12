namespace AgentSquad.Runner.Exceptions;

/// <summary>
/// Base exception class for all dashboard-related errors.
/// Provides consistent error handling and messaging across the application.
/// </summary>
public class DashboardException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DashboardException class with a message.
    /// </summary>
    public DashboardException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DashboardException class with a message and inner exception.
    /// </summary>
    public DashboardException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when dashboard data validation fails.
/// Occurs when JSON schema is invalid or required fields are missing.
/// </summary>
public class InvalidDataException : DashboardException
{
    /// <summary>
    /// Initializes a new instance of the InvalidDataException class.
    /// </summary>
    public InvalidDataException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the InvalidDataException class with an inner exception.
    /// </summary>
    public InvalidDataException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}