using System;
using System.Collections.Generic;

namespace AgentSquad.Models
{
    public class ProjectData
    {
        public Project Project { get; set; }
        public List<Milestone> Milestones { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();
        public Metrics Metrics { get; set; }
    }

    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public int CompletionPercentage { get; set; }
    }

    public enum MilestoneStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public class TaskItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TaskStatus Status { get; set; }
        public string AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
    }

    public enum TaskStatus
    {
        Shipped,
        InProgress,
        CarriedOver
    }

    public class Metrics
    {
        public int CompletionPercentage { get; set; }
        public int ShippedCount { get; set; }
        public int InProgressCount { get; set; }
        public int CarriedOverCount { get; set; }
        public decimal BurndownRate { get; set; }
    }
}