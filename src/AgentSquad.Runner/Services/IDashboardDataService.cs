using System.Threading;
using System.Threading.Tasks;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for loading and caching dashboard data from data.json
    /// </summary>
    public interface IDashboardDataService
    {
        /// <summary>
        /// Load and deserialize dashboard data from data.json.
        /// Result is cached in memory; subsequent calls return cached value.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>DashboardModel instance</returns>
        /// <exception cref="Exceptions.DashboardDataException">Thrown if file not found or JSON invalid</exception>
        Task<DashboardModel> GetDashboardDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Reload data from disk, invalidating cache.
        /// Triggered by manual "Refresh Data" button on UI.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>Completed task</returns>
        /// <exception cref="Exceptions.DashboardDataException">Thrown if file not found or JSON invalid</exception>
        Task ReloadDataAsync(CancellationToken cancellationToken = default);
    }
}