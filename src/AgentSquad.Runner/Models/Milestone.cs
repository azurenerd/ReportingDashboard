using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class Milestone
{
    [Required]
    [StringLength(256)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public DateTime Date { get; init; }

    [Required]
    [RegularExpression(@"^(Completed|On Track|At Risk)$")]
    public string Status { get; init; } = string.Empty;
}