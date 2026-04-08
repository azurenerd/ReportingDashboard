namespace AgentSquad.Runner.Data;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/// <summary>
/// Task status enumeration for project tasks.
/// </summary>
public enum TaskStatus
{
    /// <summary>Completed and delivered task.</summary>
    Shipped = 0,

    /// <summary>Task currently in progress.</summary>
    InProgress = 1,

    /// <summary>Task carried over from previous iteration or delayed.</summary>
    CarriedOver = 2
}

/// <summary>
/// Milestone status enumeration for project milestones.
/// </summary>
public enum MilestoneStatus
{
    /// <summary>Milestone has been completed.</summary>
    Completed = 0,

    /// <summary>Milestone is currently in progress.</summary>
    InProgress = 1,

    /// <summary>Milestone is pending and not yet started.</summary>
    Pending = 2
}

/// <summary>
/// Project information containing metadata and high-level details.
/// </summary>
public class ProjectInfo
{
    /// <summary>
    /// Gets or sets the project name.
    /// Example: "Q2 Mobile App Release"
    /// </summary>
    [Required(ErrorMessage = "Project name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// Example: "iOS and Android mobile app version 2.0 with new payment integration"
    /// </summary>
    [Required(ErrorMessage = "Project description is required")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    [Required(ErrorMessage = "Project start date is required")]
    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date (target completion).
    /// </summary>
    [Required(ErrorMessage = "Project end date is required")]
    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the project status.
    /// Example: "OnTrack", "AtRisk", "Delayed"
    /// </summary>
    [Required(ErrorMessage = "Project status is required")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the executive sponsor name.
    /// </summary>
    [Required(ErrorMessage = "Project sponsor is required")]
    [JsonPropertyName("sponsor")]
    public string Sponsor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project manager name.
    /// </summary>
    [Required(ErrorMessage = "Project manager is required")]
    [JsonPropertyName("projectManager")]
    public string ProjectManager { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectInfo"/> class.
    /// </summary>
    public ProjectInfo()
    {
    }
}

/// <summary>
/// Milestone representing a significant project achievement or deliverable.
/// </summary>
public class Milestone
{
    /// <summary>
    /// Gets or sets the unique milestone identifier.
    /// Example: "m1"
    /// </summary>
    [Required(ErrorMessage = "Milestone ID is required")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the milestone name.
    /// Example: "Design Review Complete"
    /// </summary>
    [Required(ErrorMessage = "Milestone name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target completion date.
    /// </summary>
    [Required(ErrorMessage = "Target date is required")]
    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// Gets or sets the actual completion date, or null if not yet completed.
    /// </summary>
    [JsonPropertyName("actualDate")]
    public DateTime? ActualDate { get; set; }

    /// <summary>
    /// Gets or sets the milestone status.
    /// </summary>
    [Required(ErrorMessage = "Milestone status is required")]
    [JsonPropertyName("status")]
    public MilestoneStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage (0-100).
    /// </summary>
    [Required(ErrorMessage = "Completion percentage is required")]
    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    [JsonPropertyName("completionPercentage")]
    public int CompletionPercentage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Milestone"/> class.
    /// </summary>
    public Milestone()
    {
    }
}

/// <summary>
/// Project task representing a unit of work with status and ownership.
/// </summary>
public class Task
{
    /// <summary>
    /// Gets or sets the unique task identifier.
    /// Example: "t1"
    /// </summary>
    [Required(ErrorMessage = "Task ID is required")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task name.
    /// Example: "API Authentication Module"
    /// </summary>
    [Required(ErrorMessage = "Task name is required")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task status.
    /// </summary>
    [Required(ErrorMessage = "Task status is required")]
    [JsonPropertyName("status")]
    public TaskStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the name of the person assigned to this task.
    /// </summary>
    [Required(ErrorMessage = "Assigned owner is required")]
    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the task due date.
    /// </summary>
    [Required(ErrorMessage = "Task due date is required")]
    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Gets or sets the estimated number of days to complete.
    /// </summary>
    [Required(ErrorMessage = "Estimated days is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Estimated days must be greater than 0")]
    [JsonPropertyName("estimatedDays")]
    public int EstimatedDays { get; set; }

    /// <summary>
    /// Gets or sets the related milestone ID.
    /// </summary>
    [JsonPropertyName("relatedMilestone")]
    public string? RelatedMilestone { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Task"/> class.
    /// </summary>
    public Task()
    {
    }
}

/// <summary>
/// Project metrics containing aggregated progress and completion data.
/// </summary>
public class ProjectMetrics
{
    /// <summary>
    /// Gets or sets the total number of tasks in the project.
    /// </summary>
    [Required(ErrorMessage = "Total tasks is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Total tasks must be non-negative")]
    [JsonPropertyName("totalTasks")]
    public int TotalTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of completed tasks.
    /// </summary>
    [Required(ErrorMessage = "Completed tasks is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Completed tasks must be non-negative")]
    [JsonPropertyName("completedTasks")]
    public int CompletedTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks currently in progress.
    /// </summary>
    [Required(ErrorMessage = "In-progress tasks is required")]
    [Range(0, int.MaxValue, ErrorMessage = "In-progress tasks must be non-negative")]
    [JsonPropertyName("inProgressTasks")]
    public int InProgressTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of tasks carried over from previous iterations.
    /// </summary>
    [Required(ErrorMessage = "Carried-over tasks is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Carried-over tasks must be non-negative")]
    [JsonPropertyName("carriedOverTasks")]
    public int CarriedOverTasks { get; set; }

    /// <summary>
    /// Gets or sets the estimated burn-down rate in tasks per day.
    /// </summary>
    [Required(ErrorMessage = "Estimated burn-down rate is required")]
    [Range(0.0, double.MaxValue, ErrorMessage = "Burn-down rate must be non-negative")]
    [JsonPropertyName("estimatedBurndownRate")]
    public double EstimatedBurndownRate { get; set; }

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    [Required(ErrorMessage = "Project start date is required")]
    [JsonPropertyName("projectStartDate")]
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date (target completion).
    /// </summary>
    [Required(ErrorMessage = "Project end date is required")]
    [JsonPropertyName("projectEndDate")]
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// Gets the calculated completion percentage (0-100).
    /// Returns 0 if total tasks is 0 to avoid division by zero.
    /// </summary>
    [JsonIgnore]
    public int CompletionPercentage
    {
        get
        {
            if (TotalTasks == 0)
            {
                return 0;
            }

            return (int)((CompletedTasks / (double)TotalTasks) * 100);
        }
    }

    /// <summary>
    /// Gets the calculated number of days remaining until project end date.
    /// Returns negative value if end date is in the past.
    /// </summary>
    [JsonIgnore]
    public int DaysRemaining
    {
        get
        {
            return (int)(ProjectEndDate - DateTime.Now).TotalDays;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectMetrics"/> class.
    /// </summary>
    public ProjectMetrics()
    {
    }
}

/// <summary>
/// Root project data container aggregating all project information, milestones, tasks, and metrics.
/// This is the top-level object for JSON deserialization from data.json.
/// </summary>
public class ProjectData
{
    /// <summary>
    /// Gets or sets the project information metadata.
    /// </summary>
    [Required(ErrorMessage = "Project information is required")]
    [JsonPropertyName("project")]
    public ProjectInfo? Project { get; set; }

    /// <summary>
    /// Gets or sets the collection of project milestones.
    /// </summary>
    [Required(ErrorMessage = "Milestones collection is required")]
    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of project tasks.
    /// </summary>
    [Required(ErrorMessage = "Tasks collection is required")]
    [JsonPropertyName("tasks")]
    public List<Task> Tasks { get; set; } = new();

    /// <summary>
    /// Gets or sets the aggregated project metrics.
    /// </summary>
    [Required(ErrorMessage = "Project metrics is required")]
    [JsonPropertyName("metrics")]
    public ProjectMetrics? Metrics { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectData"/> class.
    /// </summary>
    public ProjectData()
    {
    }
}