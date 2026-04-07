namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents the status of a project milestone in the executive dashboard.
    /// </summary>
    public enum MilestoneStatus
    {
        /// <summary>
        /// Milestone has been successfully completed and delivered.
        /// </summary>
        Completed,

        /// <summary>
        /// Milestone is currently in active development or execution.
        /// </summary>
        InProgress,

        /// <summary>
        /// Milestone is at risk and may not meet target date; intervention required.
        /// </summary>
        AtRisk,

        /// <summary>
        /// Milestone is scheduled for future work; not yet started.
        /// </summary>
        Future
    }

    /// <summary>
    /// Represents the status of a work item within the project.
    /// </summary>
    public enum WorkItemStatus
    {
        /// <summary>
        /// Work item has been completed and delivered in the current month.
        /// </summary>
        Shipped,

        /// <summary>
        /// Work item is currently being developed or worked on.
        /// </summary>
        InProgress,

        /// <summary>
        /// Work item was not completed in the planned period and has been carried over to the next period.
        /// </summary>
        CarriedOver
    }

    /// <summary>
    /// Represents the overall health status of the project.
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// Project is on track to meet all targets and milestones as scheduled.
        /// </summary>
        OnTrack,

        /// <summary>
        /// Project has identified risks that may impact delivery; attention required.
        /// </summary>
        AtRisk,

        /// <summary>
        /// Project has critical issues blocking progress; immediate intervention needed.
        /// </summary>
        Blocked
    }
}