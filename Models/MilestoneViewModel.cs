using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// View model for milestone with calculated properties for UI rendering.
    /// </summary>
    public class MilestoneViewModel
    {
        /// <summary>
        /// Unique identifier for the milestone.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name of the milestone.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Target date for the milestone (UTC).
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Enum status of the milestone.
        /// </summary>
        public MilestoneStatus Status { get; set; }

        /// <summary>
        /// Full description of the milestone.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Calculated days remaining until target date.
        /// Positive = future, negative = overdue.
        /// </summary>
        public double DaysRemaining { get; set; }

        /// <summary>
        /// Indicates if milestone is overdue (Date < DateTime.UtcNow).
        /// </summary>
        public bool IsOverdue { get; set; }

        /// <summary>
        /// Display label: "Completed", "In Progress", "Planned", or "At Risk".
        /// </summary>
        public string StatusLabel { get; set; }

        /// <summary>
        /// CSS classes for status badge styling (badge-success, badge-warning, badge-danger).
        /// </summary>
        public string CssClasses { get; set; }
    }
}