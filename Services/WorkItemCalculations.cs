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
        /// alternating CSS classes, and optional sorting by status category.
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
                throw new ArgumentNullException(nameof(items), "Items collection cannot be null.");

            if (milestones == null)
                throw new ArgumentNullException(nameof(milestones), "Milestones collection cannot be null.");

            var itemList = items.ToList();
            if (!itemList.Any())
                return new List<WorkItemViewModel>();

            var milestoneList = milestones.ToList();
            var viewModels = new List<WorkItemViewModel>();

            for (int index = 0; index < itemList.Count; index++)
            {
                var item = itemList[index];
                var milestoneName = ResolveMilestoneName(item.MilestoneId, milestoneList);
                var rowCssClass = index % 2 == 0 ? "row-even" : "row-odd";

                var viewModel = new WorkItemViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    MilestoneId = item.MilestoneId,
                    MilestoneName = milestoneName,
                    Owner = item.Owner,
                    CompletionDate = item.CompletionDate,
                    Status = item.Status,
                    RowCssClass = rowCssClass,
                    RiskBadge = string.Empty
                };

                viewModels.Add(viewModel);
            }

            return viewModels;
        }

        /// <summary>
        /// Resolves the milestone name for a given milestone ID from a collection of milestones.
        /// Returns "Unknown Milestone" if the milestone is not found or the ID is null/empty.
        /// </summary>
        /// <param name="milestoneId">The ID of the milestone to resolve.</param>
        /// <param name="milestones">The collection of milestones to search.</param>
        /// <returns>The milestone name if found, otherwise "Unknown Milestone".</returns>
        private static string ResolveMilestoneName(string milestoneId, IEnumerable<Milestone> milestones)
        {
            if (string.IsNullOrWhiteSpace(milestoneId))
                return "Unknown Milestone";

            if (milestones == null || !milestones.Any())
                return "Unknown Milestone";

            var milestone = milestones.FirstOrDefault(m => m?.Id == milestoneId);
            return milestone?.Name ?? "Unknown Milestone";
        }
    }
}