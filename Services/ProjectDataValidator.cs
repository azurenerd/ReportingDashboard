using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgentSquad.Models;

namespace AgentSquad.Services
{
    public class ProjectDataValidator
    {
        public ValidationResult Validate(ProjectData data)
        {
            var errors = new List<string>();

            if (data == null)
            {
                errors.Add("ProjectData is null.");
                return new ValidationResult { IsValid = false, Errors = errors };
            }

            errors.AddRange(ValidateProject(data.Project));
            errors.AddRange(ValidateMilestones(data.Milestones));
            errors.AddRange(ValidateTasks(data.Tasks));

            return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
        }

        private List<string> ValidateProject(Project project)
        {
            var errors = new List<string>();

            if (project == null)
            {
                errors.Add("Project metadata is missing.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(project.Name))
                errors.Add("Project name is required.");

            if (project.StartDate == default)
                errors.Add("Project startDate is required.");

            if (project.EndDate == default)
                errors.Add("Project endDate is required.");

            if (project.EndDate <= project.StartDate)
                errors.Add("Project endDate must be after startDate.");

            if (project.CompletionPercentage < 0 || project.CompletionPercentage > 100)
                errors.Add("Project completionPercentage must be between 0 and 100.");

            return errors;
        }

        private List<string> ValidateMilestones(List<Milestone> milestones)
        {
            var errors = new List<string>();

            if (milestones == null || milestones.Count == 0)
            {
                errors.Add("At least one milestone is required.");
                return errors;
            }

            if (milestones.Count < 3 || milestones.Count > 4)
                errors.Add("Milestones count should be between 3 and 4.");

            for (int i = 0; i < milestones.Count; i++)
            {
                var milestone = milestones[i];
                string prefix = $"Milestone {i + 1}: ";

                if (milestone == null)
                {
                    errors.Add($"{prefix}Milestone is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(milestone.Name))
                    errors.Add($"{prefix}Name is required.");

                if (milestone.TargetDate == default)
                    errors.Add($"{prefix}TargetDate is required.");

                if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
                    errors.Add($"{prefix}Status '{milestone.Status}' is invalid. Must be Completed, InProgress, or Pending.");

                if (milestone.CompletionPercentage < 0 || milestone.CompletionPercentage > 100)
                    errors.Add($"{prefix}CompletionPercentage must be between 0 and 100.");

                if (milestone.Status == MilestoneStatus.Completed && milestone.CompletionPercentage != 100)
                    errors.Add($"{prefix}Completed milestones must have 100% completion.");
            }

            return errors;
        }

        private List<string> ValidateTasks(List<ProjectTask> tasks)
        {
            var errors = new List<string>();

            if (tasks == null || tasks.Count == 0)
            {
                errors.Add("At least one task is required.");
                return errors;
            }

            if (tasks.Count < 8 || tasks.Count > 10)
                errors.Add("Tasks count should be between 8 and 10.");

            var statusCounts = new Dictionary<string, int>
            {
                { "Shipped", 0 },
                { "InProgress", 0 },
                { "CarriedOver", 0 }
            };

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string prefix = $"Task {i + 1}: ";

                if (task == null)
                {
                    errors.Add($"{prefix}Task is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(task.Name))
                    errors.Add($"{prefix}Name is required.");

                if (string.IsNullOrWhiteSpace(task.Description))
                    errors.Add($"{prefix}Description is required.");

                if (string.IsNullOrWhiteSpace(task.Owner))
                    errors.Add($"{prefix}Owner is required.");

                if (!Enum.IsDefined(typeof(TaskStatus), task.Status))
                    errors.Add($"{prefix}Status '{task.Status}' is invalid. Must be Shipped, InProgress, or CarriedOver.");
                else
                    statusCounts[task.Status.ToString()]++;

                if (task.EstimatedDays <= 0)
                    errors.Add($"{prefix}EstimatedDays must be greater than 0.");
            }

            if (statusCounts["Shipped"] < 3 || statusCounts["Shipped"] > 4)
                errors.Add($"Shipped tasks count should be 3-4, found {statusCounts["Shipped"]}.");

            if (statusCounts["InProgress"] < 3 || statusCounts["InProgress"] > 4)
                errors.Add($"InProgress tasks count should be 3-4, found {statusCounts["InProgress"]}.");

            if (statusCounts["CarriedOver"] < 2 || statusCounts["CarriedOver"] > 3)
                errors.Add($"CarriedOver tasks count should be 2-3, found {statusCounts["CarriedOver"]}.");

            return errors;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public string GetErrorMessage()
        {
            return string.Join(Environment.NewLine, Errors);
        }
    }
}