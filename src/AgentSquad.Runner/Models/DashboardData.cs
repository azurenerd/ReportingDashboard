using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class DashboardData
{
    [Required]
    public Project Project { get; init; } = null!;

    public List<Milestone> Milestones { get; init; } = [];

    public List<WorkItem> WorkItems { get; init; } = [];
}