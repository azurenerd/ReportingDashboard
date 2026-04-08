namespace AgentSquad.Runner.Models;

/// <summary>
/// Enumeration of task status categories for dashboard display.
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task has been shipped/delivered.
    /// </summary>
    Shipped = 0,

    /// <summary>
    /// Task is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task was carried over to the next period.
    /// </summary>
    CarriedOver = 2,

    /// <summary>
    /// Task is pending (not yet started).
    /// </summary>
    Pending = 3
}