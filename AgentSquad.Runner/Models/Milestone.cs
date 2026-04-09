using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class Milestone
{
    [Required(ErrorMessage = "Milestone ID is required")]
    [RegularExpression(@"^[a-z0-9_-]{1,50}$", ErrorMessage = "Milestone ID must be 1-50 alphanumeric characters (lowercase, digits, underscore, hyphen only)")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Milestone name is required")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Milestone name must be between 1 and 255 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Milestone date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Milestone status is required")]
    [EnumDataType(typeof(MilestoneStatus), ErrorMessage = "Milestone status must be a valid value (Planned, InProgress, Completed, AtRisk)")]
    public MilestoneStatus Status { get; set; }

    [StringLength(1000, ErrorMessage = "Milestone description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}