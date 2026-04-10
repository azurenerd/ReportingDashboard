using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class Milestone : IValidatableObject
{
    private static readonly string[] ValidStatuses = { "Completed", "On Track", "At Risk" };

    [Required(ErrorMessage = "Milestone name is required.")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Milestone name must be between 1 and 256 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Milestone date is required.")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Milestone status is required.")]
    [RegularExpression(@"^(Completed|On Track|At Risk)$", 
        ErrorMessage = "Milestone status must be 'Completed', 'On Track', or 'At Risk'.")]
    public string Status { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Milestone name cannot be empty or whitespace.", new[] { nameof(Name) });
        }

        if (Date == default)
        {
            yield return new ValidationResult("Milestone date cannot be the default DateTime value.", new[] { nameof(Date) });
        }

        if (!string.IsNullOrEmpty(Status) && !ValidStatuses.Contains(Status))
        {
            yield return new ValidationResult(
                $"Milestone status '{Status}' is not valid. Valid values are: {string.Join(", ", ValidStatuses)}.",
                new[] { nameof(Status) });
        }
    }
}