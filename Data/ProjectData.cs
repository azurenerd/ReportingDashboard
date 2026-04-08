using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Data
{
    /// <summary>
    /// Root entity containing all project information, milestones, tasks, and metrics.
    /// </summary>
    public class ProjectData
    {
        /// <summary>
        /// Gets or sets the project information.
        /// </summary>
        public ProjectInfo Project { get; set; }

        /// <summary>
        /// Gets or sets the list of project milestones.
        /// </summary>
        public List<Milestone> Milestones { get; set; }

        /// <summary>
        /// Gets or sets the list of project tasks.
        /// </summary>
        public List<Task> Tasks { get; set; }

        /// <summary>
        /// Gets or sets the project metrics.
        /// </summary>
        public ProjectMetrics Metrics { get; set; }
    }

    /// <summary>
    /// Project information and metadata.
    /// </summary>
    public class ProjectInfo
    {
        /// <summary>
        /// Gets or sets the project name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the project description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the project start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the project end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the project status (e.g., "OnTrack", "AtRisk", "Delayed").
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the executive sponsor name.
        /// </summary>
        public string Sponsor { get; set; }

        /// <summary>
        /// Gets or sets the project manager name.
        /// </summary>
        public string ProjectManager { get; set; }
    }

    /// <summary>
    /// Represents a project milestone with target and actual dates.
    /// </summary>
    public class Milestone
    {
        /// <summary>
        /// Gets or sets the unique milestone identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the milestone name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the target completion date.
        /// </summary>
        public DateTime TargetDate { get; set; }

        /// <summary>
        /// Gets or sets the actual completion date, if completed.
        /// </summary>
        public DateTime? ActualDate { get; set; }

        /// <summary>
        /// Gets or sets the milestone status.
        /// </summary>
        public MilestoneStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the completion percentage (0-100).
        /// </summary>
        public int CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Represents the status of a milestone.
    /// </summary>
    public enum MilestoneStatus
    {
        /// <summary>
        /// Milestone is completed.
        /// </summary>
        Completed = 0,

        /// <summary>
        /// Milestone is in progress.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Milestone is pending.
        /// </summary>
        Pending = 2
    }

    /// <summary>
    /// Represents a project task with assignment and status information.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Gets or sets the unique task identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the task name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the task status.
        /// </summary>
        public TaskStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the name of the person assigned to the task.
        /// </summary>
        public string AssignedTo { get; set; }

        /// <summary>
        /// Gets or sets the task due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the estimated number of days to complete the task.
        /// </summary>
        public int EstimatedDays { get; set; }

        /// <summary>
        /// Gets or sets the ID of the related milestone.
        /// </summary>
        public string RelatedMilestone { get; set; }
    }

    /// <summary>
    /// Represents the status of a task.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Task is shipped/completed.
        /// </summary>
        Shipped = 0,

        /// <summary>
        /// Task is in progress.
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// Task is carried over.
        /// </summary>
        CarriedOver = 2
    }

    /// <summary>
    /// Represents project-level metrics and progress information.
    /// </summary>
    public class ProjectMetrics
    {
        /// <summary>
        /// Gets or sets the total number of tasks.
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// Gets or sets the number of completed tasks.
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// Gets or sets the number of tasks in progress.
        /// </summary>
        public int InProgressTasks { get; set; }

        /// <summary>
        /// Gets or sets the number of carried-over tasks.
        /// </summary>
        public int CarriedOverTasks { get; set; }

        /// <summary>
        /// Gets or sets the overall completion percentage (0-100).
        /// </summary>
        public int CompletionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the estimated burn-down rate (tasks per day).
        /// </summary>
        public double EstimatedBurndownRate { get; set; }

        /// <summary>
        /// Gets or sets the project start date.
        /// </summary>
        public DateTime ProjectStartDate { get; set; }

        /// <summary>
        /// Gets or sets the project end date.
        /// </summary>
        public DateTime ProjectEndDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days remaining until project completion.
        /// </summary>
        public int DaysRemaining { get; set; }
    }
}