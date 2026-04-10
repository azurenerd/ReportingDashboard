using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class WorkItem : IValidatableObject
{
    [Required(ErrorMessage = "Work item title is required.")]
    [StringLength(512, MinimumLength = 1, ErrorMessage = "Work item title must be between 1 and 512 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Work item status is required.")]
    public WorkItemStatus Status { get; set; }

    [StringLength(256, ErrorMessage = "Assignee name cannot exceed 256 characters.")]
    public string? Assignee { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult("Work item title cannot be empty or whitespace.", new[] { nameof(Title) });
        }

        if (!Enum.IsDefined(typeof(WorkItemStatus), Status))
        {
            yield return new ValidationResult(
                $"Work item status value '{Status}' is not a valid WorkItemStatus. Valid values are: Shipped, InProgress, CarriedOver.",
                new[] { nameof(Status) });
        }

        if (Assignee != null && string.IsNullOrWhiteSpace(Assignee))
        {
            yield return new ValidationResult("Assignee cannot be empty whitespace; use null instead.", new[] { nameof(Assignee) });
        }
    }
}