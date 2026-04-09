using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class WorkItem
{
    [Required(ErrorMessage = "Work item ID is required")]
    [RegularExpression(@"^[a-z0-9_-]{1,50}$", ErrorMessage = "Work item ID must be 1-50 alphanumeric characters (lowercase, digits, underscore, hyphen only)")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Work item title is required")]
    [StringLength(512, MinimumLength = 1, ErrorMessage = "Work item title must be between 1 and 512 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Work item category is required")]
    [EnumDataType(typeof(WorkItemCategory), ErrorMessage = "Work item category must be a valid value (Shipped, InProgress, Carryover)")]
    public WorkItemCategory Category { get; set; }

    [StringLength(50, ErrorMessage = "Milestone ID cannot exceed 50 characters")]
    public string? MilestoneId { get; set; }

    [Range(0, 100, ErrorMessage = "Percent complete must be between 0 and 100")]
    public int? PercentComplete { get; set; }

    public DateTime? CompletedDate { get; set; }

    public DateTime? OriginalTarget { get; set; }

    public DateTime? NewTarget { get; set; }

    [StringLength(500, ErrorMessage = "Carryover reason cannot exceed 500 characters")]
    public string? CarryoverReason { get; set; }

    public string? AssignedTo { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}