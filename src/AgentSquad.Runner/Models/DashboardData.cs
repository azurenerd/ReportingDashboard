using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class DashboardData
{
    [Required]
    public Project Project { get; set; }
    
    public List<Milestone> Milestones { get; set; } = new();
    
    public List<WorkItem> WorkItems { get; set; } = new();
}

public class Project
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Name { get; set; }
    
    [StringLength(1024)]
    public string Description { get; set; }
}

public class Milestone
{
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string Name { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [RegularExpression(@"^(Completed|On Track|At Risk)$")]
    public string Status { get; set; }
}

public class WorkItem
{
    [Required]
    [StringLength(512, MinimumLength = 1)]
    public string Title { get; set; }
    
    [Required]
    public WorkItemStatus Status { get; set; }
    
    [StringLength(256)]
    public string Assignee { get; set; }
}

public enum WorkItemStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}