using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Utility class for calculating milestone view model properties.
    /// </summary>
    public static class MilestoneCalculations
    {
        /// <summary>
        /// Calculates milestone view model with derived properties from a milestone entity.
        /// </summary>
        /// <param name="milestone">Source milestone entity.</param>
        /// <returns>MilestoneViewModel with calculated properties.</returns>
        /// <exception cref="ArgumentNullException">Thrown if milestone is null.</exception>
        /// <exception cref="ArgumentException">Thrown if milestone date is invalid.</exception>
        public static MilestoneViewModel CalculateMilestoneViewModel(Milestone milestone)
        {
            if (milestone == null)
                throw new ArgumentNullException(nameof(milestone), "Milestone cannot be null.");

            var now = DateTime.UtcNow;
            var daysRemaining = (milestone.Date - now).TotalDays;
            var isOverdue = milestone.Date < now;

            var statusLabel = DetermineStatusLabel(milestone.Status, isOverdue, daysRemaining);
            var cssClasses = DetermineCssClasses(milestone.Status, isOverdue, daysRemaining);

            return new MilestoneViewModel
            {
                Id = milestone.Id,
                Name = milestone.Name,
                Date = milestone.Date,
                Status = milestone.Status,
                Description = milestone.Description,
                DaysRemaining = daysRemaining,
                IsOverdue = isOverdue,
                StatusLabel = statusLabel,
                CssClasses = cssClasses
            };
        }

        /// <summary>
        /// Determines the display status label based on milestone status and overdue state.
        /// Milestones >= 3 days overdue are labeled "At Risk" regardless of Status enum.
        /// </summary>
        /// <param name="status">Milestone status enum value.</param>
        /// <param name="isOverdue">Whether the milestone is overdue.</param>
        /// <param name="daysRemaining">Days remaining (negative if overdue).</param>
        /// <returns>Display label string.</returns>
        private static string DetermineStatusLabel(MilestoneStatus status, bool isOverdue, double daysRemaining)
        {
            if (isOverdue && daysRemaining <= -3)
                return "At Risk";

            return status switch
            {
                MilestoneStatus.Completed => "Completed",
                MilestoneStatus.InProgress => "In Progress",
                MilestoneStatus.Planned => "Planned",
                MilestoneStatus.AtRisk => "At Risk",
                _ => "Planned"
            };
        }

        /// <summary>
        /// Determines Bootstrap badge CSS classes for status visualization.
        /// Green (badge-success) = Completed
        /// Yellow (badge-warning) = In Progress
        /// Red (badge-danger) = At Risk or >= 3 days overdue
        /// </summary>
        /// <param name="status">Milestone status enum value.</param>
        /// <param name="isOverdue">Whether the milestone is overdue.</param>
        /// <param name="daysRemaining">Days remaining (negative if overdue).</param>
        /// <returns>CSS class string for badge styling.</returns>
        private static string DetermineCssClasses(MilestoneStatus status, bool isOverdue, double daysRemaining)
        {
            if (isOverdue && daysRemaining <= -3)
                return "badge badge-danger";

            return status switch
            {
                MilestoneStatus.Completed => "badge badge-success",
                MilestoneStatus.InProgress => "badge badge-warning",
                MilestoneStatus.AtRisk => "badge badge-danger",
                MilestoneStatus.Planned => "badge badge-secondary",
                _ => "badge badge-secondary"
            };
        }
    }
}