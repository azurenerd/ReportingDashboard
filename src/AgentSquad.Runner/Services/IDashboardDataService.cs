using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Defines the contract for dashboard data loading, caching, and real-time updates.
/// Implementations handle JSON parsing, file watching, and change notifications.
/// </summary>
public interface IDashboardDataService
{
    /// <summary>
    /// Raised when data.json is loaded or reloaded successfully or when an error occurs.
    /// Subscribers should call StateHasChanged() to trigger UI re-render.
    /// </summary>
    event Action? OnDataChanged;

    /// <summary>
    /// Gets the entire cached dashboard data model (models, milestones, work items).
    /// Returns null if data has not been loaded or loading failed.
    /// </summary>
    DashboardData? GetCurrentData();

    /// <summary>
    /// Gets the project metadata (name, description).
    /// Returns null if data has not been loaded.
    /// </summary>
    Project? GetProject();

    /// <summary>
    /// Gets all milestones sorted by date (ascending).
    /// Returns empty list if no milestones or data not loaded.
    /// </summary>
    IReadOnlyList<Milestone> GetMilestones();

    /// <summary>
    /// Gets all work items in any status.
    /// Returns empty list if no items or data not loaded.
    /// </summary>
    IReadOnlyList<WorkItem> GetWorkItems();

    /// <summary>
    /// Gets work items filtered by status (Shipped, InProgress, CarriedOver).
    /// Returns empty list if no items match or data not loaded.
    /// </summary>
    IReadOnlyList<WorkItem> GetWorkItemsByStatus(WorkItemStatus status);

    /// <summary>
    /// Gets count of work items grouped by status.
    /// Returns (0, 0, 0) if no items or data not loaded.
    /// </summary>
    (int Shipped, int InProgress, int CarriedOver) GetStatusCounts();

    /// <summary>
    /// Gets the last error message encountered during loading or parsing.
    /// Returns null if data loaded successfully or not yet loaded.
    /// </summary>
    string? GetLastError();

    /// <summary>
    /// Gets whether dashboard data has been successfully loaded and cached.
    /// Returns false if loading failed or not yet attempted.
    /// </summary>
    bool HasData { get; }
}