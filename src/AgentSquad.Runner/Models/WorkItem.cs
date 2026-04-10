using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class WorkItem
{
    [Required]
    [StringLength(512)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public WorkItemStatus Status { get; init; }

    [StringLength(256)]
    public string? Assignee { get; init; }
}