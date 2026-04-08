using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services;

public interface IDataValidator
{
    Task<DataValidationResult> ValidateDataJsonAsync(string filePath);
}

public class DataValidator : IDataValidator
{
    private readonly ILogger<DataValidator> _logger;

    public DataValidator(ILogger<DataValidator> logger)
    {
        _logger = logger;
    }

    public async Task<DataValidationResult> ValidateDataJsonAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var message = $"data.json file not found at {filePath}";
                _logger.LogError(message);
                return new DataValidationResult(false, message);
            }

            var json = await File.ReadAllTextAsync(filePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Project project;
            try
            {
                project = JsonSerializer.Deserialize<Project>(json, options);
            }
            catch (JsonException ex)
            {
                var message = $"Failed to deserialize data.json: {ex.Message}";
                _logger.LogError(message);
                return new DataValidationResult(false, message);
            }

            if (project == null)
            {
                var message = "data.json deserialized to null";
                _logger.LogError(message);
                return new DataValidationResult(false, message);
            }

            var validationErrors = new List<string>();

            ValidateProject(project, validationErrors);
            ValidateMilestones(project.Milestones, validationErrors);
            ValidateWorkItems(project.WorkItems, validationErrors);
            ValidateMetrics(project.Metrics, validationErrors);

            if (validationErrors.Any())
            {
                var message = $"Validation errors: {string.Join("; ", validationErrors)}";
                _logger.LogError(message);
                return new DataValidationResult(false, message);
            }

            _logger.LogInformation("data.json validated successfully");
            return new DataValidationResult(true, "Validation successful", project);
        }
        catch (Exception ex)
        {
            var message = $"Unexpected error during validation: {ex.Message}";
            _logger.LogError(ex, message);
            return new DataValidationResult(false, message);
        }
    }

    private void ValidateProject(Project project, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(project.Name))
        {
            errors.Add("Project name is required");
        }

        if (project.Name?.Length > 255)
        {
            errors.Add("Project name must not exceed 255 characters");
        }

        if (project.Description?.Length > 1000)
        {
            errors.Add("Project description must not exceed 1000 characters");
        }

        if (project.Milestones == null || project.Milestones.Count == 0)
        {
            errors.Add("At least one milestone is required");
        }

        if (project.WorkItems == null || project.WorkItems.Count == 0)
        {
            errors.Add("At least one work item is required");
        }

        if (project.Metrics == null)
        {
            errors.Add("Metrics object is required");
        }
    }

    private void ValidateMilestones(List<Milestone> milestones, List<string> errors)
    {
        if (milestones == null) return;

        foreach (var (milestone, index) in milestones.Select((m, i) => (m, i)))
        {
            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                errors.Add($"Milestone {index}: Name is required");
            }

            if (milestone.Name?.Length > 255)
            {
                errors.Add($"Milestone {index}: Name must not exceed 255 characters");
            }

            if (milestone.TargetDate == default)
            {
                errors.Add($"Milestone {index}: TargetDate is required");
            }

            if (!Enum.IsDefined(typeof(MilestoneStatus), milestone.Status))
            {
                errors.Add($"Milestone {index}: Invalid status value");
            }

            if (milestone.Description?.Length > 1000)
            {
                errors.Add($"Milestone {index}: Description must not exceed 1000 characters");
            }
        }
    }

    private void ValidateWorkItems(List<WorkItem> workItems, List<string> errors)
    {
        if (workItems == null) return;

        foreach (var (item, index) in workItems.Select((w, i) => (w, i)))
        {
            if (string.IsNullOrWhiteSpace(item.Title))
            {
                errors.Add($"WorkItem {index}: Title is required");
            }

            if (item.Title?.Length > 255)
            {
                errors.Add($"WorkItem {index}: Title must not exceed 255 characters");
            }

            if (!Enum.IsDefined(typeof(WorkItemStatus), item.Status))
            {
                errors.Add($"WorkItem {index}: Invalid status value");
            }

            if (item.Description?.Length > 1000)
            {
                errors.Add($"WorkItem {index}: Description must not exceed 1000 characters");
            }
        }
    }

    private void ValidateMetrics(ProjectMetrics metrics, List<string> errors)
    {
        if (metrics == null) return;

        if (metrics.CompletionPercentage < 0 || metrics.CompletionPercentage > 100)
        {
            errors.Add("CompletionPercentage must be between 0 and 100");
        }

        if (!Enum.IsDefined(typeof(HealthStatus), metrics.HealthStatus))
        {
            errors.Add("Invalid HealthStatus value");
        }

        if (metrics.VelocityCount < 0)
        {
            errors.Add("VelocityCount must be non-negative");
        }

        if (metrics.TotalMilestones < 0)
        {
            errors.Add("TotalMilestones must be non-negative");
        }
    }
}

public class DataValidationResult
{
    public bool IsValid { get; }
    public string Message { get; }
    public Project Data { get; }

    public DataValidationResult(bool isValid, string message, Project data = null)
    {
        IsValid = isValid;
        Message = message;
        Data = data;
    }
}