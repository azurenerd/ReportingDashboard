using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class Milestone
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public MilestoneStatus Status { get; set; }

    [Range(0, 100)]
    public int CompletionPercent { get; set; }
}