using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Loads and validates dashboard data from a JSON file asynchronously.
    /// </summary>
    /// <param name="filePath">Full path to the data.json file</param>
    /// <returns>Fully deserialized and validated DashboardData object</returns>
    /// <exception cref="FileNotFoundException">If file does not exist</exception>
    /// <exception cref="System.Text.Json.JsonException">If JSON is malformed</exception>
    /// <exception cref="InvalidOperationException">If JSON parses but validation fails</exception>
    /// <exception cref="Services.Exceptions.DashboardLoadException">For all load errors</exception>
    Task<DashboardData> LoadDataAsync(string filePath);

    /// <summary>
    /// Validates dashboard data against DataAnnotations and custom business rules.
    /// </summary>
    /// <param name="data">DashboardData object to validate</param>
    /// <returns>True if valid; false if validation fails</returns>
    /// <remarks>
    /// Custom validation rules enforced:
    /// - Milestones must be sorted by Date
    /// - WorkItem.MilestoneId must reference existing Milestone.Id
    /// - No duplicate WorkItem.Id or Milestone.Id values
    /// </remarks>
    Task<bool> ValidateDataAsync(DashboardData data);

    /// <summary>
    /// Monitors data.json file for changes and yields updated data asynchronously.
    /// </summary>
    /// <param name="filePath">Path to the data.json file to monitor</param>
    /// <returns>Async enumerable that yields updated DashboardData on file changes</returns>
    /// <remarks>
    /// Implementation uses FileSystemWatcher on file LastWrite event.
    /// Runs indefinitely; callers must break enumeration when done.
    /// Detects changes within ~100ms.
    /// </remarks>
    IAsyncEnumerable<DashboardData> WatchDataAsync(string filePath);
}