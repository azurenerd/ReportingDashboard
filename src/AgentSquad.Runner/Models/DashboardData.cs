using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public enum WorkItemStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}

public class Project
{
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(256, ErrorMessage = "Project name cannot exceed 256 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1024, ErrorMessage = "Project description cannot exceed 1024 characters")]
    public string? Description { get; set; }
}

public class Milestone
{
    [Required(ErrorMessage = "Milestone name is required")]
    [StringLength(256, ErrorMessage = "Milestone name cannot exceed 256 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Milestone date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Milestone status is required")]
    [RegularExpression(@"^(Completed|On Track|At Risk)$", 
        ErrorMessage = "Milestone status must be one of: Completed, On Track, At Risk")]
    public string Status { get; set; } = string.Empty;
}

public class WorkItem
{
    [Required(ErrorMessage = "Work item title is required")]
    [StringLength(512, ErrorMessage = "Work item title cannot exceed 512 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Work item status is required")]
    [EnumDataType(typeof(WorkItemStatus), ErrorMessage = "Work item status must be Shipped, InProgress, or CarriedOver")]
    public WorkItemStatus Status { get; set; }

    [StringLength(256, ErrorMessage = "Assignee name cannot exceed 256 characters")]
    public string? Assignee { get; set; }
}

public class DashboardData
{
    [Required(ErrorMessage = "Project is required")]
    public Project? Project { get; set; }

    [Required(ErrorMessage = "Milestones list is required")]
    public List<Milestone> Milestones { get; set; } = [];

    [Required(ErrorMessage = "Work items list is required")]
    public List<WorkItem> WorkItems { get; set; } = [];
}