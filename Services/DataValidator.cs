using Microsoft.Extensions.Logging;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class DataValidator : IDataValidator
    {
        private readonly ILogger<DataValidator> _logger;
        private const int MaxProjectNameLength = 100;
        private const int MaxReportingPeriodLength = 50;
        private const int MaxMilestoneIdLength = 50;
        private const int MaxMilestoneNameLength = 100;
        private const int MaxMilestoneDescriptionLength = 500;
        private const int MaxStatusItemLength = 200;
        private const int MaxKpiKeyLength = 50;

        public DataValidator(ILogger<DataValidator> logger)
        {
            _logger = logger;
        }

        public ValidationResult Validate(ProjectReport? report)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<ValidationError>() };

            if (report == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "report", Message = "Report is required." });
                _logger.LogWarning("Validation failed: Report is null.");
                return result;
            }

            ValidateProjectReport(report, result);
            ValidateStatusSnapshot(report.StatusSnapshot, result);
            ValidateMilestones(report.Milestones, result);
            ValidateKpis(report.Kpis, result);

            if (result.IsValid)
            {
                _logger.LogInformation("Validation passed: all fields valid.");
            }
            else
            {
                var firstFiveErrors = string.Join("; ", result.Errors.Take(5).Select(e => $"{e.Field}: {e.Message}"));
                _logger.LogWarning($"Validation failed with {result.Errors.Count} errors: {firstFiveErrors}");
            }

            return result;
        }

        private void ValidateProjectReport(ProjectReport report, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(report.ProjectName))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "projectName", Message = "Project name is required and cannot be empty." });
            }
            else if (report.ProjectName.Length > MaxProjectNameLength)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "projectName", Message = $"Project name cannot exceed {MaxProjectNameLength} characters." });
            }

            if (string.IsNullOrWhiteSpace(report.ReportingPeriod))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "reportingPeriod", Message = "Reporting period is required and cannot be empty." });
            }
            else if (report.ReportingPeriod.Length > MaxReportingPeriodLength)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "reportingPeriod", Message = $"Reporting period cannot exceed {MaxReportingPeriodLength} characters." });
            }

            if (report.Milestones == null || report.Milestones.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "milestones", Message = "At least one milestone is required." });
            }
        }

        private void ValidateMilestones(Milestone[]? milestones, ValidationResult result)
        {
            if (milestones == null || milestones.Length == 0)
                return;

            var seenIds = new HashSet<string>();

            for (int i = 0; i < milestones.Length; i++)
            {
                var milestone = milestones[i];

                if (string.IsNullOrWhiteSpace(milestone.Id))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].id", Message = "Milestone ID is required and cannot be empty." });
                }
                else if (milestone.Id.Length > MaxMilestoneIdLength)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].id", Message = $"Milestone ID cannot exceed {MaxMilestoneIdLength} characters." });
                }
                else if (!seenIds.Add(milestone.Id))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].id", Message = $"Milestone ID '{milestone.Id}' is not unique." });
                }

                if (string.IsNullOrWhiteSpace(milestone.Name))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].name", Message = "Milestone name is required and cannot be empty." });
                }
                else if (milestone.Name.Length > MaxMilestoneNameLength)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].name", Message = $"Milestone name cannot exceed {MaxMilestoneNameLength} characters." });
                }

                if (!IsValidIso8601Date(milestone.TargetDate))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].targetDate", Message = "Target date must be in ISO 8601 format (YYYY-MM-DD)." });
                }

                if (!IsValidStatus(milestone.Status))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].status", Message = "Status must be one of: on-track, at-risk, delayed, completed." });
                }

                if (milestone.Progress < 0 || milestone.Progress > 100)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].progress", Message = "Progress must be a value between 0 and 100." });
                }

                if (milestone.Description != null && milestone.Description.Length > MaxMilestoneDescriptionLength)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"milestones[{i}].description", Message = $"Milestone description cannot exceed {MaxMilestoneDescriptionLength} characters." });
                }
            }
        }

        private void ValidateStatusSnapshot(StatusSnapshot? snapshot, ValidationResult result)
        {
            if (snapshot == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "statusSnapshot", Message = "Status snapshot is required." });
                return;
            }

            if (snapshot.Shipped == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "statusSnapshot.shipped", Message = "Shipped array is required." });
            }
            else
            {
                ValidateStatusItems(snapshot.Shipped, "statusSnapshot.shipped", result);
            }

            if (snapshot.InProgress == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "statusSnapshot.inProgress", Message = "InProgress array is required." });
            }
            else
            {
                ValidateStatusItems(snapshot.InProgress, "statusSnapshot.inProgress", result);
            }

            if (snapshot.CarriedOver == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "statusSnapshot.carriedOver", Message = "CarriedOver array is required." });
            }
            else
            {
                ValidateStatusItems(snapshot.CarriedOver, "statusSnapshot.carriedOver", result);
            }
        }

        private void ValidateStatusItems(string[] items, string fieldPath, ValidationResult result)
        {
            if (items == null)
                return;

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (string.IsNullOrWhiteSpace(item))
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"{fieldPath}[{i}]", Message = "Status item cannot be empty." });
                }
                else if (item.Length > MaxStatusItemLength)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"{fieldPath}[{i}]", Message = $"Status item cannot exceed {MaxStatusItemLength} characters." });
                }
            }
        }

        private void ValidateKpis(Dictionary<string, int>? kpis, ValidationResult result)
        {
            if (kpis == null || kpis.Count == 0)
                return;

            foreach (var kvp in kpis)
            {
                if (kvp.Key.Length > MaxKpiKeyLength)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"kpis.{kvp.Key}", Message = $"KPI key cannot exceed {MaxKpiKeyLength} characters." });
                }

                if (kvp.Value < 0 || kvp.Value > 100)
                {
                    result.IsValid = false;
                    result.Errors.Add(new ValidationError { Field = $"kpis.{kvp.Key}", Message = "KPI value must be between 0 and 100." });
                }
            }
        }

        private bool IsValidStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            return status == "on-track" || status == "at-risk" || status == "delayed" || status == "completed";
        }

        private bool IsValidIso8601Date(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return false;

            return DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _);
        }
    }
}