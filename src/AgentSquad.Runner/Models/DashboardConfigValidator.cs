using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AgentSquad.Runner.Models;

/// <summary>
/// Validates DashboardConfig objects against schema requirements.
/// Encapsulates all validation logic for JSON structure, required fields, and field constraints.
/// </summary>
public class DashboardConfigValidator
{
    private static readonly HashSet<string> ValidMonthNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    private static readonly HashSet<string> ValidMilestoneTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "poc", "release", "checkpoint"
    };

    private const int MinYear = 2000;
    private const int MaxYear = 2099;
    private const int MaxProjectNameLength = 255;
    private const int MaxDescriptionLength = 255;
    private const int MaxLabelLength = 100;
    private const int MaxItemsPerStatus = 50;
    private const int MinQuarters = 1;
    private const int MaxQuarters = 12;
    private const int MaxMilestones = 50;

    /// <summary>
    /// Validates the complete DashboardConfig schema.
    /// </summary>
    /// <param name="config">The configuration object to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public List<string> ValidateSchema(DashboardConfig config)
    {
        var errors = new List<string>();

        if (config == null)
        {
            errors.Add("DashboardConfig cannot be null");
            return errors;
        }

        // Validate root-level required fields
        ValidateProjectName(config.ProjectName, errors);
        ValidateDescription(config.Description, errors);
        ValidateQuartersArray(config.Quarters, errors);
        ValidateMilestonesArray(config.Milestones, errors);

        // Validate quarters content
        if (config.Quarters != null && config.Quarters.Count > 0)
        {
            foreach (var (quarter, index) in config.Quarters.Select((q, i) => (q, i)))
            {
                ValidateQuarter(quarter, index, errors);
            }
        }

        // Validate milestones content
        if (config.Milestones != null && config.Milestones.Count > 0)
        {
            ValidateMilestoneUniqueness(config.Milestones, errors);
            foreach (var (milestone, index) in config.Milestones.Select((m, i) => (m, i)))
            {
                ValidateMilestone(milestone, index, errors);
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates the projectName field.
    /// </summary>
    private static void ValidateProjectName(string projectName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(projectName))
        {
            errors.Add("Required field 'projectName' is missing or empty");
            return;
        }

        if (projectName.Length > MaxProjectNameLength)
        {
            errors.Add($"Field 'projectName' exceeds maximum length of {MaxProjectNameLength} characters");
        }
    }

    /// <summary>
    /// Validates the description field.
    /// </summary>
    private static void ValidateDescription(string description, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add("Required field 'description' is missing or empty");
            return;
        }

        if (description.Length > MaxDescriptionLength)
        {
            errors.Add($"Field 'description' exceeds maximum length of {MaxDescriptionLength} characters");
        }
    }

    /// <summary>
    /// Validates the quarters array.
    /// </summary>
    private static void ValidateQuartersArray(List<Quarter> quarters, List<string> errors)
    {
        if (quarters == null || quarters.Count == 0)
        {
            errors.Add("Required field 'quarters' is missing or empty. Must contain 1-12 items");
            return;
        }

        if (quarters.Count > MaxQuarters)
        {
            errors.Add($"Field 'quarters' exceeds maximum of {MaxQuarters} items");
        }
    }

    /// <summary>
    /// Validates the milestones array (presence check only; content validated separately).
    /// </summary>
    private static void ValidateMilestonesArray(List<Milestone> milestones, List<string> errors)
    {
        if (milestones == null)
        {
            errors.Add("Required field 'milestones' is missing. Must be an array (can be empty)");
            return;
        }

        if (milestones.Count > MaxMilestones)
        {
            errors.Add($"Field 'milestones' exceeds maximum of {MaxMilestones} items");
        }
    }

    /// <summary>
    /// Validates a single Quarter object.
    /// </summary>
    private static void ValidateQuarter(Quarter quarter, int quarterIndex, List<string> errors)
    {
        if (quarter == null)
        {
            errors.Add($"Quarter at index {quarterIndex} is null");
            return;
        }

        var prefix = $"Quarter {quarterIndex}";

        // Validate month
        if (string.IsNullOrWhiteSpace(quarter.Month))
        {
            errors.Add($"{prefix}: Required field 'month' is missing or empty");
        }
        else if (!IsValidMonthName(quarter.Month))
        {
            errors.Add($"{prefix}: Field 'month' has invalid value '{quarter.Month}'. Must be a valid month name (January-December)");
        }

        // Validate year
        if (!IsValidYear(quarter.Year))
        {
            errors.Add($"{prefix}: Field 'year' has invalid value {quarter.Year}. Must be in range {MinYear}-{MaxYear}");
        }

        // Validate status arrays
        ValidateStatusArray(quarter.Shipped, $"{prefix}.shipped", errors);
        ValidateStatusArray(quarter.InProgress, $"{prefix}.inProgress", errors);
        ValidateStatusArray(quarter.Carryover, $"{prefix}.carryover", errors);
        ValidateStatusArray(quarter.Blockers, $"{prefix}.blockers", errors);
    }

    /// <summary>
    /// Validates a status array (shipped, inProgress, carryover, blockers).
    /// </summary>
    private static void ValidateStatusArray(List<string> items, string fieldName, List<string> errors)
    {
        if (items == null)
        {
            return;
        }

        if (items.Count > MaxItemsPerStatus)
        {
            errors.Add($"{fieldName}: Exceeds maximum of {MaxItemsPerStatus} items");
        }
    }

    /// <summary>
    /// Validates milestone uniqueness (IDs must be unique within the milestones array).
    /// </summary>
    private static void ValidateMilestoneUniqueness(List<Milestone> milestones, List<string> errors)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (milestone, index) in milestones.Select((m, i) => (m, i)))
        {
            if (string.IsNullOrWhiteSpace(milestone?.Id))
            {
                errors.Add($"Milestone at index {index}: Required field 'id' is missing or empty");
                continue;
            }

            if (!ids.Add(milestone.Id))
            {
                errors.Add($"Milestone with id '{milestone.Id}' appears more than once. Milestone IDs must be unique");
            }
        }
    }

    /// <summary>
    /// Validates a single Milestone object.
    /// </summary>
    private static void ValidateMilestone(Milestone milestone, int milestoneIndex, List<string> errors)
    {
        if (milestone == null)
        {
            errors.Add($"Milestone at index {milestoneIndex} is null");
            return;
        }

        var prefix = $"Milestone {milestoneIndex}";

        // Validate id (already checked in uniqueness validation, but check here too for completeness)
        if (string.IsNullOrWhiteSpace(milestone.Id))
        {
            errors.Add($"{prefix}: Required field 'id' is missing or empty");
        }

        // Validate label
        if (string.IsNullOrWhiteSpace(milestone.Label))
        {
            errors.Add($"{prefix}: Required field 'label' is missing or empty");
        }
        else if (milestone.Label.Length > MaxLabelLength)
        {
            errors.Add($"{prefix}: Field 'label' exceeds maximum length of {MaxLabelLength} characters");
        }

        // Validate date
        if (string.IsNullOrWhiteSpace(milestone.Date))
        {
            errors.Add($"{prefix}: Required field 'date' is missing or empty");
        }
        else if (!IsValidDate(milestone.Date))
        {
            errors.Add($"{prefix}: Field 'date' has invalid value '{milestone.Date}'. Must be a valid ISO 8601 date (YYYY-MM-DD)");
        }

        // Validate type
        if (string.IsNullOrWhiteSpace(milestone.Type))
        {
            errors.Add($"{prefix}: Required field 'type' is missing or empty");
        }
        else if (!IsValidMilestoneType(milestone.Type))
        {
            errors.Add($"{prefix}: Field 'type' has invalid value '{milestone.Type}'. Must be one of: poc, release, checkpoint");
        }
    }

    /// <summary>
    /// Checks if a month name is valid.
    /// </summary>
    private static bool IsValidMonthName(string month)
    {
        return ValidMonthNames.Contains(month);
    }

    /// <summary>
    /// Checks if a year is within the valid range.
    /// </summary>
    private static bool IsValidYear(int year)
    {
        return year >= MinYear && year <= MaxYear;
    }

    /// <summary>
    /// Checks if a date string is a valid ISO 8601 date (YYYY-MM-DD).
    /// </summary>
    private static bool IsValidDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return false;
        }

        return DateTime.TryParseExact(
            dateString,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out _);
    }

    /// <summary>
    /// Checks if a milestone type is valid.
    /// </summary>
    private static bool IsValidMilestoneType(string type)
    {
        return ValidMilestoneTypes.Contains(type);
    }
}