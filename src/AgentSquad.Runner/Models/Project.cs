using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class Project : IValidatableObject
{
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Project name must be between 1 and 256 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1024, ErrorMessage = "Project description cannot exceed 1024 characters.")]
    public string? Description { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Project name cannot be empty or whitespace.", new[] { nameof(Name) });
        }

        if (Description != null && string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult("Project description cannot be empty whitespace; use null instead.", new[] { nameof(Description) });
        }
    }
}