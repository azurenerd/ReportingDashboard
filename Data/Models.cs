namespace AgentSquad.Runner.Data;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Task status enumeration for project tasks.
/// </summary>
public enum TaskStatus
{
    /// <summary>Completed and delivered task.</summary>
    Shipped = 0,

    /// <summary>Task currently in progress.</summary>
    InProgress = 1,

    /// <summary>Task carried over from previous iteration or delayed.</summary>
    CarriedOver = 2
}

/// <summary>
/// Milestone status enumeration for project milestones.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>Milestone has been completed.</summary>
    Completed = 0,

    /// <summary>Milestone is currently in progress.</summary>
    InProgress = 1,

    /// <summary>Milestone is pending and not yet started.</summary>
    Pending = 2
}