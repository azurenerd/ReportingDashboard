using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AgentSquad.Runner.Exceptions;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Validators
{
    /// <summary>
    /// Validates DashboardModel for required fields and data integrity
    /// </summary>
    public static class DashboardModelValidator
    {
        private static readonly string[] ValidCategories = { "shipped", "inprogress", "carryover", "blockers" };
        private static readonly string[] ValidTypes = { "checkpoint", "poc", "release" };
        private static readonly string[] ValidMonths = { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };

        public static void Validate(DashboardModel model)
        {
            if (model == null)
                throw new ValidationException("Dashboard model is null");

            ValidateProject(model.Project);
            ValidateMilestones(model.Milestones);
            ValidateStatusRows(model.StatusRows);
            ValidateNowMarker(model.NowMarker);
        }

        private static void ValidateProject(ProjectInfo? project)
        {
            if (project == null)
                throw new ValidationException("Project object is required");

            if (string.IsNullOrWhiteSpace(project.Name))
                throw new ValidationException("Project.Name is required and cannot be empty");

            if (string.IsNullOrWhiteSpace(project.Subtitle))
                throw new ValidationException("Project.Subtitle is required and cannot be empty");
        }

        private static void ValidateMilestones(List<Milestone>? milestones)
        {
            if (milestones == null || milestones.Count == 0)
                throw new ValidationException("At least one milestone is required");

            foreach (var milestone in milestones)
            {
                if (string.IsNullOrWhiteSpace(milestone.Id))
                    throw new ValidationException("Milestone.Id is required");

                if (string.IsNullOrWhiteSpace(milestone.Title))
                    throw new ValidationException($"Milestone '{milestone.Id}' requires a Title");

                if (string.IsNullOrWhiteSpace(milestone.Date))
                    throw new ValidationException($"Milestone '{milestone.Id}' requires a Date in ISO 8601 format");

                if (!IsValidIso8601Date(milestone.Date))
                    throw new ValidationException(
                        $"Milestone '{milestone.Id}' has invalid date format. Use ISO 8601 (yyyy-MM-dd): {milestone.Date}");

                if (string.IsNullOrWhiteSpace(milestone.Type))
                    throw new ValidationException($"Milestone '{milestone.Id}' requires a Type");

                if (!ValidTypes.Contains(milestone.Type))
                    throw new ValidationException(
                        $"Milestone '{milestone.Id}' has invalid type '{milestone.Type}'. Valid types: {string.Join(", ", ValidTypes)}");

                if (!string.IsNullOrWhiteSpace(milestone.Color) && !IsValidHexColor(milestone.Color))
                    throw new ValidationException(
                        $"Milestone '{milestone.Id}' has invalid color format. Use hex format (#RRGGBB): {milestone.Color}");
            }
        }

        private static void ValidateStatusRows(List<StatusRow>? statusRows)
        {
            if (statusRows == null || statusRows.Count < 4)
                throw new ValidationException("Four status rows are required (shipped, inprogress, carryover, blockers)");

            var foundCategories = new HashSet<string>();

            foreach (var row in statusRows)
            {
                if (string.IsNullOrWhiteSpace(row.Category))
                    throw new ValidationException("StatusRow.Category is required");

                if (!ValidCategories.Contains(row.Category))
                    throw new ValidationException(
                        $"Invalid status category: '{row.Category}'. Valid values: {string.Join(", ", ValidCategories)}");

                foundCategories.Add(row.Category);

                if (row.Items != null)
                {
                    foreach (var item in row.Items)
                    {
                        if (string.IsNullOrWhiteSpace(item.Month))
                            throw new ValidationException(
                                $"StatusRow '{row.Category}' contains an item with missing Month");

                        if (!ValidMonths.Contains(item.Month))
                            throw new ValidationException(
                                $"StatusRow '{row.Category}' contains invalid month '{item.Month}'. Valid months: {string.Join(", ", ValidMonths)}");

                        if (string.IsNullOrWhiteSpace(item.Value))
                            throw new ValidationException(
                                $"StatusRow '{row.Category}' contains an item with missing Value for month {item.Month}");

                        if (item.Value.Length > 100)
                            throw new ValidationException(
                                $"StatusRow '{row.Category}' item value exceeds 100 characters: {item.Value.Substring(0, 50)}...");
                    }
                }
            }

            foreach (var category in ValidCategories)
            {
                if (!foundCategories.Contains(category))
                    throw new ValidationException($"Required status category '{category}' is missing");
            }
        }

        private static void ValidateNowMarker(string? nowMarker)
        {
            if (string.IsNullOrWhiteSpace(nowMarker))
                throw new ValidationException("NowMarker date is required");

            if (!IsValidIso8601Date(nowMarker))
                throw new ValidationException(
                    $"NowMarker date must be in ISO 8601 format (yyyy-MM-dd): {nowMarker}");
        }

        private static bool IsValidIso8601Date(string dateString)
        {
            return DateTime.TryParseExact(
                dateString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out _);
        }

        private static bool IsValidHexColor(string color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            if (!color.StartsWith("#") || color.Length != 7)
                return false;

            return color.Skip(1).All(c => 
                (c >= '0' && c <= '9') || 
                (c >= 'A' && c <= 'F') || 
                (c >= 'a' && c <= 'f'));
        }
    }
}