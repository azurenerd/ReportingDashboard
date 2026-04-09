using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class ProjectMetadata
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Range(0, 100, ErrorMessage = "Overall status percent complete must be between 0 and 100")]
    public int? OverallStatusPercentComplete { get; set; }
}