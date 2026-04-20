using System.Text.RegularExpressions;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public static class DashboardDataValidator
{
    private static readonly Regex HexColorRegex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

    public static IReadOnlyList<string> Validate(DashboardData data)
    {
        var errors = new List<string>();

        if (data is null)
        {
            errors.Add("DashboardData is null.");
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
            errors.Add("project is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(project.Title))
        {
            errors.Add("project.title is required and cannot be empty.");
        }

        if (!string.IsNullOrWhiteSpace(project.BacklogUrl))
        {
            if (!Uri.TryCreate(project.BacklogUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                errors.Add("project.backlogUrl must be a valid absolute http(s) URL.");
            }
        }
    }

    private static void ValidateTimeline(Timeline? timeline, List<string> errors)
    {
        if (timeline is null)
        {
            errors.Add("timeline is required.");
            return;
        }

        if (timeline.Start >= timeline.End)
        {
            errors.Add($"timeline.start ({timeline.Start:yyyy-MM-dd}) must be before timeline.end ({timeline.End:yyyy-MM-dd}).");
        }

        if (timeline.Lanes is null || timeline.Lanes.Count == 0)
        {
            errors.Add("timeline.lanes must contain between 1 and 6 lanes.");
            return;
        }

        if (timeline.Lanes.Count > 6)
        {
            errors.Add($"timeline.lanes count ({timeline.Lanes.Count}) exceeds maximum of 6.");
        }

        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < timeline.Lanes.Count; i++)
        {
            var lane = timeline.Lanes[i];
            if (lane is null)
            {
                errors.Add($"timeline.lanes[{i}] is null.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(lane.Id))
            {
                errors.Add($"timeline.lanes[{i}].id is required.");
            }
            else if (!seenIds.Add(lane.Id))
            {
                errors.Add($"timeline.lanes[{i}].id '{lane.Id}' is duplicated; lane ids must be unique.");
            }

            if (string.IsNullOrWhiteSpace(lane.Color) || !HexColorRegex.IsMatch(lane.Color))
            {
                errors.Add($"timeline.lanes[{i}].color '{lane.Color}' must match pattern #RRGGBB.");
            }

            if (lane.Milestones is not null)
            {
                for (int m = 0; m < lane.Milestones.Count; m++)
                {
                    var milestone = lane.Milestones[m];
                    if (milestone is null)
                    {
                        errors.Add($"timeline.lanes[{i}].milestones[{m}] is null.");
                        continue;
                    }

                    if (milestone.Date < timeline.Start || milestone.Date > timeline.End)
                    {
                        errors.Add($"timeline.lanes[{i}].milestones[{m}].date ({milestone.Date:yyyy-MM-dd}) is outside timeline range [{timeline.Start:yyyy-MM-dd}, {timeline.End:yyyy-MM-dd}].");
                    }

                    if (!Enum.IsDefined(typeof(MilestoneType), milestone.Type))
                    {
                        errors.Add($"timeline.lanes[{i}].milestones[{m}].type '{milestone.Type}' is not a valid milestone type.");
                    }
                }
            }
        }
    }

    private static void ValidateHeatmap(Heatmap? heatmap, List<string> errors)
    {
        if (heatmap is null)
        {
            errors.Add("heatmap is required.");
            return;
        }

        if (heatmap.Months is null || heatmap.Months.Count == 0)
        {
            errors.Add("heatmap.months must be non-empty.");
            return;
        }

        if (heatmap.Months.Count != 4)
        {
            errors.Add($"heatmap.months length ({heatmap.Months.Count}) must equal 4 for v1.");
        }

        if (heatmap.CurrentMonthIndex.HasValue &&
            (heatmap.CurrentMonthIndex.Value < 0 || heatmap.CurrentMonthIndex.Value >= heatmap.Months.Count))
        {
            errors.Add($"heatmap.currentMonthIndex ({heatmap.CurrentMonthIndex}) must be in range [0, {heatmap.Months.Count}).");
        }

        if (heatmap.MaxItemsPerCell < 1)
        {
            errors.Add($"heatmap.maxItemsPerCell ({heatmap.MaxItemsPerCell}) must be >= 1.");
        }

        if (heatmap.Rows is null || heatmap.Rows.Count == 0)
        {
            errors.Add("heatmap.rows is required and must contain 4 rows.");
            return;
        }

        if (heatmap.Rows.Count != 4)
        {
            errors.Add($"heatmap.rows count ({heatmap.Rows.Count}) must be exactly 4.");
        }

        var seenCategories = new HashSet<HeatmapCategory>();
        for (int r = 0; r < heatmap.Rows.Count; r++)
        {
            var row = heatmap.Rows[r];
            if (row is null)
            {
                errors.Add($"heatmap.rows[{r}] is null.");
                continue;
            }

            if (!Enum.IsDefined(typeof(HeatmapCategory), row.Category))
            {
                errors.Add($"heatmap.rows[{r}].category '{row.Category}' is not a valid category.");
            }
            else if (!seenCategories.Add(row.Category))
            {
                errors.Add($"heatmap.rows[{r}].category '{row.Category}' is duplicated; categories must be unique.");
            }

            if (row.Cells is null)
            {
                errors.Add($"heatmap.rows[{r}].cells is required.");
                continue;
            }

            if (row.Cells.Count != heatmap.Months.Count)
            {
                errors.Add($"heatmap.rows[{r}].cells count ({row.Cells.Count}) must equal heatmap.months count ({heatmap.Months.Count}).");
            }
        }

        var expected = new[] { HeatmapCategory.Shipped, HeatmapCategory.InProgress, HeatmapCategory.Carryover, HeatmapCategory.Blockers };
        foreach (var cat in expected)
        {
            if (!seenCategories.Contains(cat))
            {
                errors.Add($"heatmap.rows is missing required category '{cat}'.");
            }
        }
    }
}