using System;
using System.Collections.Generic;
using System.Linq;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public static class WorkItemCalculations
    {
        /// <summary>
        /// Transforms a collection of work items into view models with milestone name resolution,
        /// alternating CSS classes, and sorting by status category.
        /// </summary>
        /// <param name="items">The work items to transform.</param>
        /// <param name="milestones">The milestone reference data for name lookup.</param>
        /// <returns>A list of work item view models with resolved milestone names and styling.</returns>
        /// <exception cref="ArgumentNullException">Thrown if items or milestones collection is null.</exception>
        public static List<WorkItemViewModel> MapWorkItemViewModels(
            IEnumerable<WorkItem> items,
            IEnumerable<Milestone> milestones)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (milestones == null)
                throw new ArgumentNullException(nameof(milestones));

            // Implementation will be added in Step 2
            return new List<WorkItemViewModel>();
        }

        /// <summary>
        /// Resolves the milestone name for a given milestone ID from a collection of milestones.
        /// </summary>
        /// <param name="milestoneId">The ID of the milestone to resolve.</param>
        /// <param name="milestones">The collection of milestones to search.</param>
        /// <returns>The milestone name if found, otherwise "Unknown Milestone".</returns>
        private static string ResolveMilestoneName(string milestoneId, IEnumerable<Milestone> milestones)
        {
            if (string.IsNullOrWhiteSpace(milestoneId) || milestones == null)
                return "Unknown Milestone";

            var milestone = milestones.FirstOrDefault(m => m.Id == milestoneId);
            return milestone?.Name ?? "Unknown Milestone";
        }
    }
}