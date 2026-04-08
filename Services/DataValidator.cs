using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class DataValidator : IDataValidator
{
    private readonly ILogger<DataValidator> _logger;

    public DataValidator(ILogger<DataValidator> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateProjectData(Project? project)
    {
        var result = new ValidationResult { IsValid = true };

        if (project == null)
        {
            result.AddError("PROJECT_NULL", "Project data is null");
            _logger.LogError("Validation failed: Project is null");
            return result;
        }

        ValidateProjectName(project, result);
        ValidateMilestones(project, result);
        ValidateWorkItems(project, result);
        ValidateCompletionPercentage(project, result);
        ValidateHealthStatus(project, result);

        if (!result.IsValid)
        {
            _logger.LogWarning("Validation failed with {ErrorCount} errors: {Errors}",
                result.Errors.Count, string.Join("; ", result.Errors.Select(e => e.ToString())));
        }
        else
        {
            _logger.LogInformation("Project data validation passed");
        }

        return result;
    }

    private void ValidateProjectName(Project project, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            result.AddError("NAME_EMPTY", "Project name cannot be empty or whitespace", nameof(Project.Name));
        }
    }

    private void ValidateMilestones(Project project, ValidationResult result)
    {
        if (project.Milestones == null)
        {
            result.AddError("MILESTONES_NULL", "Milestones collection cannot be null", nameof(Project.Milestones));
            return;
        }

        if (project.Milestones.Count == 0)
        {
            result.AddError("MILESTONES_EMPTY", "At least one milestone is required", nameof(Project.Milestones));
            return;
        }

        for (int i = 0; i < project.Milestones.Count; i++)
        {
            var milestone = project.Milestones[i];

            if (milestone == null)
            {
                result.AddError("MILESTONE_NULL", $"Milestone at index {i} is null", $"{nameof(Project.Milestones)}[{i}]");
                continue;
            }

            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                result.AddError("MILESTONE_NAME_EMPTY",
                    $"Milestone at index {i} has empty or whitespace name",
                    $"{nameof(Project.Milestones)}[{i}].{nameof(Milestone.Name)}");
            }

            if (milestone.TargetDate == default)
            {
                result.AddError("MILESTONE_DATE_INVALID",
                    $"Milestone '{milestone.Name}' has invalid target date",
                    $"{nameof(Project.Milestones)}[{i}].{nameof(Milestone.TargetDate)}");
            }

            if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
            {
                result.AddError("MILESTONE_STATUS_INVALID",
                    $"Milestone '{milestone.Name}' has invalid status",
                    $"{nameof(Project.Milestones)}[{i}].{nameof(Milestone.Status)}");
            }
        }
    }

    private void ValidateWorkItems(Project project, ValidationResult result)
    {
        if (project.WorkItems == null)
        {
            result.AddError("WORKITEMS_NULL", "WorkItems collection cannot be null", nameof(Project.WorkItems));
            return;
        }

        for (int i = 0; i < project.WorkItems.Count; i++)
        {
            var workItem = project.WorkItems[i];

            if (workItem == null)
            {
                result.AddError("WORKITEM_NULL", $"WorkItem at index {i} is null", $"{nameof(Project.WorkItems)}[{i}]");
                continue;
            }

            if (string.IsNullOrWhiteSpace(workItem.Title))
            {
                result.AddError("WORKITEM_TITLE_EMPTY",
                    $"WorkItem at index {i} has empty or whitespace title",
                    $"{nameof(Project.WorkItems)}[{i}].{nameof(WorkItem.Title)}");
            }

            if (!Enum.IsDefined(typeof(WorkItemStatus), workItem.Status))
            {
                result.AddError("WORKITEM_STATUS_INVALID",
                    $"WorkItem '{workItem.Title}' has invalid status",
                    $"{nameof(Project.WorkItems)}[{i}].{nameof(WorkItem.Status)}");
            }
        }
    }

    private void ValidateCompletionPercentage(Project project, ValidationResult result)
    {
        if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
        {
            result.AddError("COMPLETION_PERCENTAGE_INVALID",
                $"Completion percentage must be between 0 and 100, got {project.CompletionPercentage}",
                nameof(Project.CompletionPercentage));
        }
    }

    private void ValidateHealthStatus(Project project, ValidationResult result)
    {
        if (!Enum.IsDefined(typeof(HealthStatus), project.HealthStatus))
        {
            result.AddError("HEALTH_STATUS_INVALID",
                $"Health status '{project.HealthStatus}' is not a valid HealthStatus value",
                nameof(Project.HealthStatus));
        }
    }
}