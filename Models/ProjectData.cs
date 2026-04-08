using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    public class ProjectData
    {
        [JsonPropertyName("projectInfo")]
        public ProjectInfo ProjectInfo { get; set; }

        [JsonPropertyName("projectMetrics")]
        public ProjectMetrics ProjectMetrics { get; set; }

        [JsonPropertyName("milestones")]
        public List<Milestone> Milestones { get; set; } = new();

        [JsonPropertyName("tasks")]
        public List<ProjectTask> Tasks { get; set; } = new();
    }

    public class ProjectInfo
    {
        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; }

        [JsonPropertyName("sponsor")]
        public string Sponsor { get; set; }

        [JsonPropertyName("projectManager")]
        public string ProjectManager { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("projectStartDate")]
        public string ProjectStartDate { get; set; }

        [JsonPropertyName("projectEndDate")]
        public string ProjectEndDate { get; set; }
    }

    public class ProjectMetrics
    {
        [JsonPropertyName("totalTasks")]
        public int TotalTasks { get; set; }

        [JsonPropertyName("completedTasks")]
        public int CompletedTasks { get; set; }

        [JsonPropertyName("inProgressTasks")]
        public int InProgressTasks { get; set; }

        [JsonPropertyName("carriedOverTasks")]
        public int CarriedOverTasks { get; set; }

        [JsonPropertyName("overallCompletionPercentage")]
        public int OverallCompletionPercentage { get; set; }

        [JsonPropertyName("onSchedule")]
        public bool OnSchedule { get; set; }

        [JsonPropertyName("riskLevel")]
        public string RiskLevel { get; set; }
    }

    public class Milestone
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("targetDate")]
        public string TargetDate { get; set; }

        [JsonPropertyName("actualDate")]
        public string ActualDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("completionPercentage")]
        public int CompletionPercentage { get; set; }
    }

    public class ProjectTask
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("assignedTo")]
        public string AssignedTo { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("dueDate")]
        public string DueDate { get; set; }

        [JsonPropertyName("estimatedDays")]
        public int EstimatedDays { get; set; }

        [JsonPropertyName("relatedMilestone")]
        public string RelatedMilestone { get; set; }
    }
}