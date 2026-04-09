using System;
using System.Collections.Generic;
using System.Linq;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Tests.Integration;

namespace AgentSquad.Runner.Services
{
    public static class WorkItemCalculations
    {
        public static List<WorkItemViewModel> MapWorkItemViewModels(
            IEnumerable<WorkItem> items,
            IEnumerable<Milestone> milestones,
            WorkItemType type)
        {
            if (items == null)
                return new List<WorkItemViewModel>();

            var itemList = items.ToList();
            if (!itemList.Any())
                return new List<WorkItemViewModel>();

            var milestoneList = milestones?.ToList() ?? new List<Milestone>();
            var viewModels = new List<WorkItemViewModel>();

            int index = 0;
            foreach (var item in itemList)
            {
                var milestone = milestoneList.FirstOrDefault(m => m.Id == item.MilestoneId);
                var milestoneName = milestone?.Name ?? "Unknown Milestone";

                var vm = new WorkItemViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    MilestoneId = item.MilestoneId,
                    MilestoneName = milestoneName,
                    Owner = item.Owner,
                    CompletionDate = item.CompletionDate,
                    Status = type.ToString(),
                    RowCssClass = index % 2 == 0 ? "row-even" : "row-odd",
                    RiskBadge = CalculateRiskBadge(milestone)
                };

                viewModels.Add(vm);
                index++;
            }

            return SortByType(viewModels, type, milestoneList);
        }

        private static List<WorkItemViewModel> SortByType(
            List<WorkItemViewModel> items,
            WorkItemType type,
            List<Milestone> milestones)
        {
            return type switch
            {
                WorkItemType.Shipped => items.OrderByDescending(x => x.CompletionDate).ToList(),
                WorkItemType.InProgress => SortByMilestoneDate(items, milestones),
                WorkItemType.CarriedOver => SortByMilestoneDate(items, milestones),
                _ => items
            };
        }

        private static List<WorkItemViewModel> SortByMilestoneDate(
            List<WorkItemViewModel> items,
            List<Milestone> milestones)
        {
            return items.OrderBy(x =>
            {
                var milestone = milestones.FirstOrDefault(m => m.Id == x.MilestoneId);
                return milestone?.DueDate ?? DateTime.MaxValue;
            }).ToList();
        }

        private static string CalculateRiskBadge(Milestone milestone)
        {
            if (milestone == null)
                return "On Track";

            var daysUntilDue = (milestone.DueDate - DateTime.UtcNow).TotalDays;

            if (daysUntilDue < 0)
                return "Overdue";
            if (daysUntilDue <= 7)
                return "At Risk";
            return "On Track";
        }
    }
}