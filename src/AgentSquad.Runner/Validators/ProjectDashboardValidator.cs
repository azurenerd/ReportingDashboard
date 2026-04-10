namespace AgentSquad.Runner.Validators;

using System.Text.Json;
using AgentSquad.Runner.Models;

/// <summary>
/// Schema validator for ProjectDashboard.
/// 
/// Enforces:
///   - Required fields (ProjectName)
///   - Enum constraints (Milestone.Status)
///   - Uniqueness constraints (WorkItem.Id globally unique)
///   - Cross-list consistency
/// 
/// Called by ProjectDataService during InitializeAsync().
/// Throws JsonException with descriptive messages on validation failure.
/// </summary>
public static class ProjectDashboardValidator
{
    /// <summary>
    /// Validate dashboard schema.
    /// 
    /// Validations (in order):
    ///   1. Dashboard object not null
    ///   2. ProjectName required and non-empty
    ///   3. Each Milestone.Status in allowed values: "Completed", "OnTrack", "AtRisk", "Delayed"
    ///   4. All WorkItem.Id values globally unique across Shipped, InProgress, CarriedOver
    ///   5. Each WorkItem.Id non-empty (required)
    /// 
    /// Throws on first validation failure with descriptive message.
    /// Messages are user-friendly (suitable for display in error alert).
    /// </summary>
    /// <param name="dashboard">ProjectDashboard to validate</param>
    /// <exception cref="JsonException">If any validation fails</exception>
    public static void Validate(ProjectDashboard? dashboard)
    {
        if (dashboard == null)
        {
            throw new JsonException("Dashboard data is null");
        }

        ValidateProjectName(dashboard);
        ValidateMilestoneStatuses(dashboard);
        ValidateWorkItemIdUniqueness(dashboard);
    }

    /// <summary>
    /// Validate ProjectName is required and non-empty.
    /// </summary>
    private static void ValidateProjectName(ProjectDashboard dashboard)
    {
        if (string.IsNullOrWhiteSpace(dashboard.ProjectName))
        {
            throw new JsonException("ProjectName is required and cannot be empty");
        }
    }

    /// <summary>
    /// Validate all Milestone.Status values are in the allowed set.
    /// 
    /// Valid status values: "Completed", "OnTrack", "AtRisk", "Delayed"
    /// </summary>
    private static void ValidateMilestoneStatuses(ProjectDashboard dashboard)
    {
        var allowedStatuses = new HashSet<string>
        {
            "Completed",
            "OnTrack",
            "AtRisk",
            "Delayed",
        };

        foreach (var milestone in dashboard.Milestones ?? new())
        {
            if (!allowedStatuses.Contains(milestone.Status))
            {
                throw new JsonException(
                    $"Invalid milestone status: '{milestone.Status}'. "
                        + $"Allowed values: {string.Join(", ", allowedStatuses)}"
                );
            }
        }
    }

    /// <summary>
    /// Validate WorkItem.Id is globally unique across all three status lists.
    /// 
    /// Collects all IDs from Shipped, InProgress, CarriedOver and checks for duplicates.
    /// Also validates each ID is non-empty (required).
    /// </summary>
    private static void ValidateWorkItemIdUniqueness(ProjectDashboard dashboard)
    {
        var seenIds = new HashSet<string>();

        var allItems = new List<WorkItem>();
        allItems.AddRange(dashboard.Shipped ?? new());
        allItems.AddRange(dashboard.InProgress ?? new());
        allItems.AddRange(dashboard.CarriedOver ?? new());

        foreach (var item in allItems)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                throw new JsonException("WorkItem.Id is required and cannot be empty");
            }

            if (!seenIds.Add(item.Id))
            {
                throw new JsonException(
                    $"Duplicate WorkItem.Id: '{item.Id}'. "
                        + "All work item IDs must be globally unique."
                );
            }
        }
    }
}