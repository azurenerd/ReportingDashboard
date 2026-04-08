namespace AgentSquad.Runner.Data
{
    /// <summary>
    /// Root entity containing all project data, milestones, tasks, and metrics.
    /// Deserialized directly from data.json.
    /// </summary>
    public class ProjectData
    {
        /// <summary>
        /// Project metadata: name, description, status, dates, and personnel.
        /// </summary>
        public ProjectInfo? Project { get; set; }

        /// <summary>
        /// List of project milestones with target dates and completion status.
        /// </summary>
        public List<Milestone> Milestones { get; set; } = new();

        /// <summary>
        /// List of tasks organized by status (shipped, in-progress, carried-over).
        /// </summary>
        public List<Task> Tasks { get; set; } = new();

        /// <summary>
        /// Aggregated project metrics: completion %, task counts, burn-down rate.
        /// </summary>
        public ProjectMetrics? Metrics { get; set; }
    }

    /// <summary>
    /// Project information: high-level metadata about the project.
    /// </summary>
    public class ProjectInfo
    {
        /// <summary>
        /// Project name, e.g., "Q2 Mobile App Release"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Brief project description, e.g., "iOS and Android mobile app v2.0"
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Project start date (kickoff).
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Project target end date (completion deadline).
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Current project status: "OnTrack", "AtRisk", or "Delayed"
        /// </summary>
        public string Status { get; set; } = "OnTrack";

        /// <summary>
        /// Executive sponsor name or title.
        /// </summary>
        public string Sponsor { get; set; } = string.Empty;

        /// <summary>
        /// Project manager name.
        /// </summary>
        public string ProjectManager { get; set; } = string.Empty;
    }

    /// <summary>
    /// Milestone representing a significant project deliverable or phase gate.
    /// </summary>
    public class Milestone
    {
        /// <summary>
        /// Unique identifier for the milestone, e.g., "m1", "m2".
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Milestone name, e.g., "Design Review Complete"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Target date for milestone completion.
        /// </summary>
        public DateTime TargetDate { get; set; }

        /// <summary>
        /// Actual completion date. Null if not yet completed.
        /// </summary>
        public DateTime? ActualDate { get; set; }

        /// <summary>
        /// Current milestone status: Completed, InProgress, or Pending.
        /// </summary>
        public MilestoneStatus Status { get; set; }

        /// <summary>
        /// Completion percentage (0-100). Used for in-progress milestones.
        /// </summary>
        public int CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Enumeration for milestone status values.
    /// </summary>
    public enum MilestoneStatus
    {
        /// <summary>Milestone has been completed and verified.</summary>
        Completed = 0,

        /// <summary>Milestone is currently in progress.</summary>
        InProgress = 1,

        /// <summary>Milestone has not yet started or been scheduled.</summary>
        Pending = 2
    }

    /// <summary>
    /// Task representing work items with status, owner, and due date.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Unique identifier for the task, e.g., "t1", "t2".
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Task name or description, e.g., "API Authentication Module"
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Current task status: Shipped, InProgress, or CarriedOver.
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// Name of the team member or group assigned to this task.
        /// </summary>
        public string AssignedTo { get; set; } = string.Empty;

        /// <summary>
        /// Task due date or deadline.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Estimated number of days to complete the task.
        /// Used for burn-down calculations.
        /// </summary>
        public int EstimatedDays { get; set; }

        /// <summary>
        /// Reference to the related milestone ID (e.g., "m1").
        /// Links tasks to milestones for organizational purposes.
        /// </summary>
        public string RelatedMilestone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Enumeration for task status values.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>Task has been completed and delivered.</summary>
        Shipped = 0,

        /// <summary>Task is actively being worked on.</summary>
        InProgress = 1,

        /// <summary>Task was not completed in the planned sprint/phase and carried over.</summary>
        CarriedOver = 2
    }

    /// <summary>
    /// Aggregated metrics representing overall project health and progress.
    /// </summary>
    public class ProjectMetrics
    {
        /// <summary>
        /// Total number of tasks in the project.
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// Number of tasks that have been completed and shipped.
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// Number of tasks currently in progress.
        /// </summary>
        public int InProgressTasks { get; set; }

        /// <summary>
        /// Number of tasks that were carried over from a previous iteration.
        /// </summary>
        public int CarriedOverTasks { get; set; }

        /// <summary>
        /// Project completion percentage (0-100).
        /// Calculated as (CompletedTasks / TotalTasks) * 100.
        /// </summary>
        public int CompletionPercentage { get; set; }

        /// <summary>
        /// Estimated burn-down rate in tasks per day.
        /// Used for projecting completion dates.
        /// </summary>
        public double EstimatedBurndownRate { get; set; }

        /// <summary>
        /// Project start date.
        /// </summary>
        public DateTime ProjectStartDate { get; set; }

        /// <summary>
        /// Project target end date.
        /// </summary>
        public DateTime ProjectEndDate { get; set; }

        /// <summary>
        /// Number of calendar days remaining until the project end date.
        /// Calculated as (ProjectEndDate - Now).Days
        /// </summary>
        public int DaysRemaining { get; set; }
    }
}