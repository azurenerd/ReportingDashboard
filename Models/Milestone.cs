using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a project milestone with target date and status.
    /// </summary>
    public class Milestone
    {
        /// <summary>
        /// Unique identifier for the milestone (alphanumeric with hyphens/underscores).
        /// </summary>
        [Required]
        [RegularExpression(@"^[a-z0-9_-]{1,50}$")]
        public string Id { get; set; }

        /// <summary>
        /// Display name of the milestone.
        /// </summary>
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Target date for milestone completion (UTC, ISO 8601).
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Current status of the milestone.
        /// </summary>
        [Required]
        [EnumDataType(typeof(MilestoneStatus))]
        public MilestoneStatus Status { get; set; }

        /// <summary>
        /// Detailed description of the milestone objectives.
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }
    }

    /// <summary>
    /// Enumeration of possible milestone status values.
    /// </summary>
    public enum MilestoneStatus
    {
        /// <summary>
        /// Milestone is planned but not yet started.
        /// </summary>
        Planned = 0,

        /// <summary>
        /// Work on milestone is currently in progress.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Milestone has been completed.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Milestone is at risk (may not meet target date or objectives).
        /// </summary>
        AtRisk = 3
    }
}