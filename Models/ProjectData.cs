using System;
using System.Collections.Generic;

namespace AgentSquad.Dashboard.Models
{
    public class ProjectData
    {
        public string ProjectName { get; set; }
        public string Sponsor { get; set; }
        public string ProjectManager { get; set; }
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public string Status { get; set; }
        public List<Milestone> Milestones { get; set; }
        public List<ProjectTask> Tasks { get; set; }
        public ProjectMetrics Metrics { get; set; }
    }

    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
        public int CompletionPercentage { get; set; }
    }

    public class ProjectTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Owner { get; set; }
    }

    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
        public int TotalTasks { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksInProgress { get; set; }
        public int TasksCarriedOver { get; set; }
    }

    public class TaskStatusSummary
    {
        public int ShippedCount { get; set; }
        public int InProgressCount { get; set; }
        public int CarriedOverCount { get; set; }
    }
}