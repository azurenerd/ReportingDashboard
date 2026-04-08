namespace AgentSquad.Models
{
    /// <summary>
    /// Project health metrics and KPI data model.
    /// Aggregates key performance indicators for executive dashboard display.
    /// </summary>
    public class ProjectMetrics
    {
        /// <summary>
        /// Overall project completion percentage (0-100).
        /// </summary>
        public int CompletionPercentage { get; set; }

        /// <summary>
        /// Project health status indicator.
        /// </summary>
        public HealthStatus HealthStatus { get; set; }

        /// <summary>
        /// Number of work items completed this month (velocity metric).
        /// </summary>
        public int VelocityThisMonth { get; set; }

        /// <summary>
        /// Total number of milestones in project.
        /// </summary>
        public int TotalMilestones { get; set; }

        /// <summary>
        /// Number of completed milestones.
        /// </summary>
        public int CompletedMilestones { get; set; }
    }

    /// <summary>
    /// Project health status enum for KPI indicators.
    /// </summary>
    public enum HealthStatus
    {
        OnTrack = 0,
        AtRisk = 1,
        Blocked = 2
    }
}