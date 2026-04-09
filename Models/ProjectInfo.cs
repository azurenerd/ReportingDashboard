using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class ProjectInfo
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ProjectStatus Status { get; set; }
}