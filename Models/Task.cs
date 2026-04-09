using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class ProjectTask
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TaskStatus Status { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Owner { get; set; }

    public string? Month { get; set; }
}