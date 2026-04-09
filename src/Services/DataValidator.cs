using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AgentSquad.Runner.Interfaces;
using AgentSquad.Runner.Models;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Runner.Services
{
    public class DataValidator : IDataValidator
    {
        private readonly ILogger<DataValidator> _logger;

        public DataValidator(ILogger<DataValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ValidationResult Validate(ProjectReport report)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<ValidationError>() };

            if (report == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "root", Message = "Project report is null" });
                return result;
            }

            // Validate ProjectName
            if (string.IsNullOrWhiteSpace(report.ProjectName))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "projectName", Message = "Project name is required and cannot be empty." });
            }
            else if (report.ProjectName.Length > 100)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "projectName", Message = "Project name cannot exceed 100 characters." });
            }

            // Validate ReportingPeriod
            if (string.IsNullOrWhiteSpace(report.ReportingPeriod))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "reportingPeriod", Message = "Reporting period is required." });
            }
            else if (report.ReportingPeriod.Length > 50)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "reportingPeriod", Message = "Reporting period cannot exceed 50 characters." });
            }

            // Validate Milestones
            if (report.Milestones == null || report.Milestones.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "milestones", Message = "At least one milestone is required." });
            }
            else
            {
                var milestoneIds = new HashSet<string>();
                for (int i = 0; i < report.Milestones.Length; i++)
                {
                    var milestone = report.Milestones[i];
                    ValidateMilestone(milestone, i, result, milestoneIds);
                }
            }

            // Validate StatusSnapshot
            if (report.StatusSnapshot == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = "statusSnapshot", Message = "Status snapshot is required." });
            }
            else
            {
                ValidateStatusSnapshot(report.StatusSnapshot, result);
            }

            // Validate KPIs (optional)
            if (report.Kpis != null)
            {
                foreach (var kpi in report.Kpis)
                {
                    if (kpi.Value < 0 || kpi.Value > 100)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError 
                        { 
                            Field = $"kpis[{kpi.Key}]", 
                            Message = $"KPI '{kpi.Key}': Value must be a number between 0 and 100." 
                        });
                    }
                }
            }

            if (!result.IsValid)
            {
                _logger.LogWarning($"Validation failed with {result.Errors.Count} errors");
            }

            return result;
        }

        private void ValidateMilestone(Milestone milestone, int index, ValidationResult result, HashSet<string> milestoneIds)
        {
            if (milestone == null)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}]", Message = "Milestone cannot be null." });
                return;
            }

            // Validate id
            if (string.IsNullOrWhiteSpace(milestone.Id))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].id", Message = "Milestone id is required." });
            }
            else if (milestone.Id.Length > 50)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].id", Message = "Milestone id cannot exceed 50 characters." });
            }
            else if (milestoneIds.Contains(milestone.Id))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].id", Message = $"Milestone id '{milestone.Id}' is not unique." });
            }
            else
            {
                milestoneIds.Add(milestone.Id);
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(milestone.Name))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].name", Message = "Milestone name is required." });
            }
            else if (milestone.Name.Length > 100)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].name", Message = "Milestone name cannot exceed 100 characters." });
            }

            // Validate targetDate
            if (string.IsNullOrWhiteSpace(milestone.TargetDate))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].targetDate", Message = "Target date is required." });
            }
            else if (!DateTime.TryParseExact(milestone.TargetDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].targetDate", Message = $"Invalid target date format. Use YYYY-MM-DD (e.g., 2026-05-15)." });
            }

            // Validate status
            if (string.IsNullOrWhiteSpace(milestone.Status))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].status", Message = "Status is required." });
            }
            else if (!IsValidStatus(milestone.Status))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].status", Message = $"Invalid status '{milestone.Status}'. Must be one of: on-track, at-risk, delayed, completed." });
            }

            // Validate progress
            if (milestone.Progress < 0 || milestone.Progress > 100)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].progress", Message = "Progress must be a number between 0 and 100." });
            }

            // Validate description (optional)
            if (!string.IsNullOrEmpty(milestone.Description) && milestone.Description.Length > 500)
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError { Field = $"milestones[{index}].description", Message = "Description cannot exceed 500 characters." });
            }
        }

        private void ValidateStatusSnapshot(StatusSnapshot snapshot, ValidationResult result)
        {
            if (snapshot.Shipped != null)
            {
                for (int i = 0; i < snapshot.Shipped.Count; i++)
                {
                    if (string.IsNullOrEmpty(snapshot.Shipped[i]) || snapshot.Shipped[i].Length > 200)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Field = $"statusSnapshot.shipped[{i}]", Message = "Shipped item must be non-empty and not exceed 200 characters." });
                    }
                }
            }

            if (snapshot.InProgress != null)
            {
                for (int i = 0; i < snapshot.InProgress.Count; i++)
                {
                    if (string.IsNullOrEmpty(snapshot.InProgress[i]) || snapshot.InProgress[i].Length > 200)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Field = $"statusSnapshot.inProgress[{i}]", Message = "In-progress item must be non-empty and not exceed 200 characters." });
                    }
                }
            }

            if (snapshot.CarriedOver != null)
            {
                for (int i = 0; i < snapshot.CarriedOver.Count; i++)
                {
                    if (string.IsNullOrEmpty(snapshot.CarriedOver[i]) || snapshot.CarriedOver[i].Length > 200)
                    {
                        result.IsValid = false;
                        result.Errors.Add(new ValidationError { Field = $"statusSnapshot.carriedOver[{i}]", Message = "Carried-over item must be non-empty and not exceed 200 characters." });
                    }
                }
            }
        }

        private bool IsValidStatus(string status)
        {
            return status == "on-track" || status == "at-risk" || status == "delayed" || status == "completed";
        }
    }
}