using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class DashboardData : IValidatableObject
{
    [Required(ErrorMessage = "Project is required.")]
    public Project? Project { get; set; }

    public List<Milestone> Milestones { get; set; } = new();

    public List<WorkItem> WorkItems { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Project == null)
        {
            yield return new ValidationResult("Project cannot be null.", new[] { nameof(Project) });
            yield break;
        }

        var projectResults = ((IValidatableObject)Project).Validate(context).ToList();
        foreach (var result in projectResults)
        {
            yield return result;
        }

        if (Milestones == null)
        {
            yield return new ValidationResult("Milestones list cannot be null; use empty list instead.", new[] { nameof(Milestones) });
        }
        else
        {
            for (int i = 0; i < Milestones.Count; i++)
            {
                var milestone = Milestones[i];
                if (milestone == null)
                {
                    yield return new ValidationResult($"Milestone at index {i} is null.", new[] { nameof(Milestones) });
                    continue;
                }

                var milestoneResults = ((IValidatableObject)milestone).Validate(context).ToList();
                foreach (var result in milestoneResults)
                {
                    var newMemberNames = result.MemberNames.Count() > 0
                        ? result.MemberNames.Select(m => $"{nameof(Milestones)}[{i}].{m}").ToList()
                        : new List<string> { $"{nameof(Milestones)}[{i}]" };
                    yield return new ValidationResult(result.ErrorMessage, newMemberNames);
                }
            }
        }

        if (WorkItems == null)
        {
            yield return new ValidationResult("WorkItems list cannot be null; use empty list instead.", new[] { nameof(WorkItems) });
        }
        else
        {
            for (int i = 0; i < WorkItems.Count; i++)
            {
                var workItem = WorkItems[i];
                if (workItem == null)
                {
                    yield return new ValidationResult($"WorkItem at index {i} is null.", new[] { nameof(WorkItems) });
                    continue;
                }

                var workItemResults = ((IValidatableObject)workItem).Validate(context).ToList();
                foreach (var result in workItemResults)
                {
                    var newMemberNames = result.MemberNames.Count() > 0
                        ? result.MemberNames.Select(m => $"{nameof(WorkItems)}[{i}].{m}").ToList()
                        : new List<string> { $"{nameof(WorkItems)}[{i}]" };
                    yield return new ValidationResult(result.ErrorMessage, newMemberNames);
                }
            }
        }
    }
}