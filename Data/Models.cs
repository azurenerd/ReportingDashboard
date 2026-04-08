using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Data;

public class ProjectData
{
    public ProjectInfo Project { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<Task> Tasks { get; set; }
    public ProjectMetrics Metrics { get; set; }
}

public class ProjectInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
    public string Sponsor { get; set; }
    public string ProjectManager { get; set; }
}

public class Milestone
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public int CompletionPercentage { get; set; }
}

public enum MilestoneStatus
{
    Completed = 0,
    InProgress = 1,
    Pending = 2
}

public class Task
{
    public string Id { get; set; }
    public string Name { get; set; }
    public TaskStatus Status { get; set; }
    public string AssignedTo { get; set; }
    public DateTime DueDate { get; set; }
    public int EstimatedDays { get; set; }
    public string RelatedMilestone { get; set; }
}

public enum TaskStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}

public class ProjectMetrics
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CarriedOverTasks { get; set; }
    public int CompletionPercentage { get; set; }
    public double EstimatedBurndownRate { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }
    public int DaysRemaining { get; set; }
}

public class DataLoadException : Exception
{
    public DataLoadException(string message) : base(message) { }
}