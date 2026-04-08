namespace AgentSquad.Models
{
    /// <summary>
    /// Work item model representing a task or deliverable in the project.
    /// Used for tracking status: shipped, in-progress, or carried over.
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Title or name of the work item. Required field.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Brief description of the work item. Optional field; may be null.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Current status of the work item (Shipped, InProgress, CarriedOver).
        /// </summary>
        public WorkItemStatus Status { get; set; }

        /// <summary>
        /// Team member or team assigned to this work item. Optional field; may be null.
        /// </summary>
        public string AssignedTo { get; set; }
    }

    /// <summary>
    /// Enum representing the status category of a work item.
    /// Used for grouping items in dashboard display.
    /// </summary>
    public enum WorkItemStatus
    {
        /// <summary>
        /// Work item shipped or completed this month.
        /// </summary>
        Shipped = 0,

        /// <summary>
        /// Work item currently in progress.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Work item carried over from previous period.
        /// </summary>
        CarriedOver = 2
    }
}