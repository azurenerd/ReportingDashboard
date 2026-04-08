using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSquad.Models
{
    public class ProjectData
    {
        [JsonPropertyName("project")]
        public Project Project { get; set; }

        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new();

        [JsonPropertyName("tasks")]
        public List<ProjectTask> Tasks { get; set; } = new();

        public ProjectMetrics Metrics { get; set; }
    }

    public class Project
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }
    }

    public class Milestone
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("targetDate")]
        public DateTime TargetDate { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MilestoneStatus Status { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }
    }

    public class ProjectTask
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatus Status { get; set; }

        [JsonPropertyName("estimatedDays")]
        public int EstimatedDays { get; set; }
    }

    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
    }

    public enum MilestoneStatus
    {
        Completed,
        InProgress,
        Pending
    }

    public enum TaskStatus
    {
        Shipped,
        InProgress,
        CarriedOver
    }
}