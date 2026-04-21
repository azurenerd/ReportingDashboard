using System.Text.RegularExpressions;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public static partial class DashboardDataValidator
{
    [GeneratedRegex(@"^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorRegex();

    public static IReadOnlyList<string> Validate(DashboardData data)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(data.Project.Title))
            errors.Add("project.title must not be empty.");

        if (!string.IsNullOrEmpty(data.Project.BacklogUrl)
            && !Uri.TryCreate(data.Project.BacklogUrl, UriKind.Absolute, out _))
            errors.Add($"project.backlogUrl is not a valid absolute URI: '{data.Project.BacklogUrl}'.");

        if (data.Timeline.Start >= data.Timeline.End)
            errors.Add($"timeline.start ({data.Timeline.Start}) must be before timeline.end ({data.Timeline.End}).");

        if (data.Timeline.Lanes.Count < 1 || data.Timeline.Lanes.Count > 6)
            errors.Add($"timeline.lanes count must be 1..6, got {data.Timeline.Lanes.Count}.");

        var laneIds = new HashSet<string>();
        foreach (var lane in data.Timeline.Lanes)
        {
            if (string.IsNullOrWhiteSpace(lane.Id))
                errors.Add("Each timeline lane must have a non-empty id.");

            if (!laneIds.Add(lane.Id))
                errors.Add($"Duplicate lane id: '{lane.Id}'.");

            if (!HexColorRegex().IsMatch(lane.Color))
                errors.Add($"Lane '{lane.Id}' color '{lane.Color}' must match #RRGGBB format.");

            foreach (var milestone in lane.Milestones)
            {
                if (milestone.Date < data.Timeline.Start || milestone.Date > data.Timeline.End)
                    errors.Add($"Milestone '{milestone.Label}' date {milestone.Date} is outside timeline range [{data.Timeline.Start}, {data.Timeline.End}].");
            }
        }

        if (data.Heatmap.Months.Count != 4)
            errors.Add($"heatmap.months must have exactly 4 entries, got {data.Heatmap.Months.Count}.");

        if (data.Heatmap.Rows.Count != 4)
            errors.Add($"heatmap.rows must have exactly 4 rows, got {data.Heatmap.Rows.Count}.");

        var requiredCategories = new HashSet<HeatmapCategory>
        {
            HeatmapCategory.Shipped, HeatmapCategory.InProgress,
            HeatmapCategory.Carryover, HeatmapCategory.Blockers
        };

        foreach (var row in data.Heatmap.Rows)
        {
            requiredCategories.Remove(row.Category);
            if (row.Cells.Count != data.Heatmap.Months.Count)
                errors.Add($"Heatmap row '{row.Category}' has {row.Cells.Count} cells but {data.Heatmap.Months.Count} months are defined.");
        }

        if (requiredCategories.Count > 0)
            errors.Add($"Missing heatmap categories: {string.Join(", ", requiredCategories)}.");

        if (data.Heatmap.CurrentMonthIndex.HasValue)
        {
            var idx = data.Heatmap.CurrentMonthIndex.Value;
            if (idx < 0 || idx >= data.Heatmap.Months.Count)
                errors.Add($"heatmap.currentMonthIndex ({idx}) is out of range [0, {data.Heatmap.Months.Count}).");
        }

        if (data.Heatmap.MaxItemsPerCell < 1)
            errors.Add($"heatmap.maxItemsPerCell must be >= 1, got {data.Heatmap.MaxItemsPerCell}.");

        return errors;
    }
}