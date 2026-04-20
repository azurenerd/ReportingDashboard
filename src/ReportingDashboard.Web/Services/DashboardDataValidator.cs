using System.Text.RegularExpressions;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Pure, stateless v1 validator for a deserialized <see cref="DashboardData"/>.
/// Returns a list of human-readable error strings; empty means valid.
/// </summary>
public static class DashboardDataValidator
{
    private static readonly Regex HexColor = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    private static readonly HashSet<HeatmapCategory> RequiredCategories = new()
    {
        HeatmapCategory.Shipped,
        HeatmapCategory.InProgress,
        HeatmapCategory.Carryover,
        HeatmapCategory.Blockers
    };

    public static IReadOnlyList<string> Validate(DashboardData? data)
    {
        var errors = new List<string>();
        if (data is null)
        {
            errors.Add("data is null");
            return errors;
        }

        ValidateProject(data.Project, errors);
        ValidateTimeline(data.Timeline, errors);
        ValidateHeatmap(data.Heatmap, errors);

        return errors;
    }

    private static void ValidateProject(Project? project, List<string> errors)
    {
        if (project is null)
        {
            errors.Add("project is required");
            return;
        }
        if (string.IsNullOrWhiteSpace(project.Title))
        {
            errors.Add("project.title must be non-empty");
        }
        if (!string.IsNullOrWhiteSpace(project.BacklogUrl) &&
            !Uri.TryCreate(project.BacklogUrl, UriKind.Absolute, out _))
        {
            errors.Add($"project.backlogUrl is not a valid absolute URI: '{project.BacklogUrl}'");
        }
    }

    private static void ValidateTimeline(Timeline? timeline, List<string> errors)
    {
        if (timeline is null)
        {
            errors.Add("timeline is required");
            return;
        }

        if (timeline.Start >= timeline.End)
        {
            errors.Add($"timeline.start ({timeline.Start:yyyy-MM-dd}) must be before timeline.end ({timeline.End:yyyy-MM-dd})");
        }

        var lanes = timeline.Lanes ?? Array.Empty<TimelineLane>();
        if (lanes.Count < 1 || lanes.Count > 6)
        {
            errors.Add($"timeline.lanes must have 1..6 entries (got {lanes.Count})");
        }

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < lanes.Count; i++)
        {
            var lane = lanes[i];
            var prefix = $"timeline.lanes[{i}]";
            if (string.IsNullOrWhiteSpace(lane.Id))
            {
                errors.Add($"{prefix}.id must be non-empty");
            }
            else if (!seenIds.Add(lane.Id))
            {
                errors.Add($"{prefix}.id '{lane.Id}' is duplicated");
            }

            if (string.IsNullOrWhiteSpace(lane.Color) || !HexColor.IsMatch(lane.Color))
            {
                errors.Add($"{prefix}.color must match ^#[0-9A-Fa-f]{{6}}$ (got '{lane.Color}')");
            }

            var milestones = lane.Milestones ?? Array.Empty<Milestone>();
            for (var m = 0; m < milestones.Count; m++)
            {
                var ms = milestones[m];
                if (timeline.Start < timeline.End &&
                    (ms.Date < timeline.Start || ms.Date > timeline.End))
                {
                    errors.Add(
                        $"{prefix}.milestones[{m}].date ({ms.Date:yyyy-MM-dd}) is outside timeline range " +
                        $"[{timeline.Start:yyyy-MM-dd}, {timeline.End:yyyy-MM-dd}]");
                }
            }
        }
    }

    private static void ValidateHeatmap(Heatmap? heatmap, List<string> errors)
    {
        if (heatmap is null)
        {
            errors.Add("heatmap is required");
            return;
        }

        var months = heatmap.Months ?? Array.Empty<string>();
        if (months.Count == 0)
        {
            errors.Add("heatmap.months must have at least one entry");
        }

        var rows = heatmap.Rows ?? Array.Empty<HeatmapRow>();
        var seenCats = new HashSet<HeatmapCategory>();
        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (!seenCats.Add(row.Category))
            {
                errors.Add($"heatmap.rows[{i}].category '{row.Category}' is duplicated");
            }

            var cells = row.Cells ?? Array.Empty<IReadOnlyList<string>>();
            if (months.Count > 0 && cells.Count != months.Count)
            {
                errors.Add(
                    $"heatmap.rows[{i}] ({row.Category}) has {cells.Count} cells but months.Length is {months.Count}");
            }
        }

        foreach (var required in RequiredCategories)
        {
            if (!seenCats.Contains(required))
            {
                errors.Add($"heatmap.rows is missing required category '{required}'");
            }
        }

        if (heatmap.CurrentMonthIndex is { } idx)
        {
            if (idx < 0 || idx >= months.Count)
            {
                errors.Add(
                    $"heatmap.currentMonthIndex ({idx}) must be in [0, {months.Count}) or null");
            }
        }

        if (heatmap.MaxItemsPerCell < 1)
        {
            errors.Add($"heatmap.maxItemsPerCell must be >= 1 (got {heatmap.MaxItemsPerCell})");
        }
    }
}
