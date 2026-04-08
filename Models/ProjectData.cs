using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Dashboard.Models
{
    public class ProjectData
    {
        [Required(ErrorMessage = "Project name is required")]
        public string ProjectName { get; set; }

        [Required(ErrorMessage = "Sponsor is required")]
        public string Sponsor { get; set; }

        [Required(ErrorMessage = "Project manager is required")]
        public string ProjectManager { get; set; }

        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }

        [Required(ErrorMessage = "Project status is required")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Milestones list is required")]
        public List<Milestone> Milestones { get; set; } = new List<Milestone>();

        [Required(ErrorMessage = "Tasks list is required")]
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        [Required(ErrorMessage = "Metrics are required")]
        public ProjectMetrics Metrics { get; set; }
    }

    public enum MilestoneStatus
    {
        Completed,
        InProgress,
        Pending
    }

    public class Milestone
    {
        [Required(ErrorMessage = "Milestone name is required")]
        public string Name { get; set; }

        public DateTime TargetDate { get; set; }

        [Required(ErrorMessage = "Milestone status is required")]
        public MilestoneStatus Status { get; set; }

        [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
        public int CompletionPercentage { get; set; }
    }

    public enum TaskStatus
    {
        Shipped,
        InProgress,
        CarriedOver
    }

    public class ProjectTask
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Task name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Task status is required")]
        public TaskStatus Status { get; set; }

        [Required(ErrorMessage = "Task owner is required")]
        public string Owner { get; set; }
    }

    public class ProjectMetrics
    {
        [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
        public int CompletionPercentage { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total tasks must be non-negative")]
        public int TotalTasks { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Completed tasks must be non-negative")]
        public int TasksCompleted { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "In-progress tasks must be non-negative")]
        public int TasksInProgress { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Carried-over tasks must be non-negative")]
        public int TasksCarriedOver { get; set; }
    }
}