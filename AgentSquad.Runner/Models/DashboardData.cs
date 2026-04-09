using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class DashboardData
{
    [Required(ErrorMessage = "Project metadata is required")]
    public ProjectMetadata Project { get; set; } = new();

    [Required(ErrorMessage = "Milestones collection is required")]
    [MinLength(1, ErrorMessage = "At least one milestone is required")]
    [MaxLength(100, ErrorMessage = "Maximum 100 milestones allowed")]
    public List<Milestone> Milestones { get; set; } = new();

    [Required(ErrorMessage = "Work items collection is required")]
    [MinLength(1, ErrorMessage = "At least one work item is required")]
    [MaxLength(500, ErrorMessage = "Maximum 500 work items allowed")]
    public List<WorkItem> WorkItems { get; set; } = new();

    public DashboardSummary Summary { get; set; } = new();

    public DateTime DataLoadedAt { get; set; } = DateTime.UtcNow;
}