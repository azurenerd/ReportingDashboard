using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public enum MilestoneStatus
{
    Completed,
    InProgress,
    AtRisk,
    Future
}

public enum WorkItemStatus
{
    ShippedThisMonth,
    InProgress,
    CarriedOver
}

public enum HealthStatus
{
    OnTrack,
    AtRisk,
    Blocked
}

public class Milestone
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; }

    [Required]
    public DateTime TargetDate { get; set; }

    [Required]
    public MilestoneStatus Status { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }
}

public class WorkItem
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Title { get; set; }

    [Required]
    public WorkItemStatus Status { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }
}

public class ProjectMetrics
{
    [Required]
    [Range(0, 100)]
    public int CompletionPercentage { get; set; }

    [Required]
    public HealthStatus HealthStatus { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int VelocityCount { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int TotalMilestones { get; set; }
}

public class Project
{
    [Required]
    [StringLength(255, MinimumLength = 1)]
    public string Name { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    public List<Milestone> Milestones { get; set; } = new();

    [Required]
    public List<WorkItem> WorkItems { get; set; } = new();

    [Required]
    public ProjectMetrics Metrics { get; set; }
}