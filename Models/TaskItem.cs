namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents a task item with status, owner, and metadata.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Unique identifier for the task.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Task name or title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Name of the person assigned to this task.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the task (Shipped, InProgress, CarriedOver).
    /// </summary>
    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    /// <summary>
    /// Optional description of the task.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Task creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}