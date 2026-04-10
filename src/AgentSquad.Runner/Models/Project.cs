using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class Project
{
    [Required]
    [StringLength(256)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1024)]
    public string? Description { get; init; }
}