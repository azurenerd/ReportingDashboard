using System;
using System.Collections.Generic;

namespace AgentSquad.Models
{
  public class ProjectData
  {
    public string ProjectName { get; set; }
    public DateTime ProjectStartDate { get; set; }
    public DateTime ProjectEndDate { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<TaskItem> Tasks { get; set; } = new();
    public int CompletionPercentage { get; set; }
    public int OnTrackPercentage { get; set; }
    public int AtRiskPercentage { get; set; }
    public int BlockedPercentage { get; set; }

    public int ShippedCount => ShippedTasks?.Count ?? 0;
    public int InProgressCount => InProgressTasks?.Count ?? 0;
    public int CarriedOverCount => CarriedOverTasks?.Count ?? 0;

    public List<TaskItem> ShippedTasks { get; set; } = new();
    public List<TaskItem> InProgressTasks { get; set; } = new();
    public List<TaskItem> CarriedOverTasks { get; set; } = new();
  }
}