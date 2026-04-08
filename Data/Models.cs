using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Data;

public class ProjectData
{
    public ProjectInfo? Project { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<Task> Tasks { get; set; } = new();
    public ProjectMetrics? Summary { get; set; }
}

public class ProjectInfo
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class Milestone
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public DateTime TargetDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public int CompletionPercentage { get; set; }
}

public class Task
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Owner { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime DueDate { get; set; }
}

public class ProjectMetrics
{
    public int CompletionPercentage { get; set; }
    public int TasksShipped { get; set; }
    public int TasksInProgress { get; set; }
    public int TasksCarriedOver { get; set; }
}

public enum MilestoneStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}

public enum TaskStatus
{
    Shipped,
    InProgress,
    CarriedOver
}