using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    public class ProjectData
    {
        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; }

        [JsonPropertyName("projectStartDate")]
        public string ProjectStartDate { get; set; }

        [JsonPropertyName("projectEndDate")]
        public string ProjectEndDate { get; set; }

        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new();

        [JsonPropertyName("tasks")]
        public List<ProjectTask> Tasks { get; set; } = new();
    }

    public class Milestone
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("targetDate")]
        public string TargetDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }
    }

    public class ProjectTask
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}