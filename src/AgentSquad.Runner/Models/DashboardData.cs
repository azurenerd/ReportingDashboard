using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class DashboardData
{
    [JsonPropertyName("project")]
    [Required(ErrorMessage = "Project is required")]
    public Project Project { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public List<WorkItem> WorkItems { get; set; } = new();
}

public class Project
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Project name is required")]
    [StringLength(256, ErrorMessage = "Project name must be 256 characters or less")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [StringLength(1024, ErrorMessage = "Project description must be 1024 characters or less")]
    public string? Description { get; set; }
}

public class Milestone
{
    [JsonPropertyName("name")]
    [Required(ErrorMessage = "Milestone name is required")]
    [StringLength(256, ErrorMessage = "Milestone name must be 256 characters or less")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    [Required(ErrorMessage = "Milestone date is required")]
    public DateTime Date { get; set; }

    [JsonPropertyName("status")]
    [Required(ErrorMessage = "Milestone status is required")]
    [RegularExpression(@"^(Completed|On Track|At Risk)$", 
        ErrorMessage = "Milestone status must be 'Completed', 'On Track', or 'At Risk'")]
    public string Status { get; set; } = "On Track";
}

public class WorkItem
{
    [JsonPropertyName("title")]
    [Required(ErrorMessage = "Work item title is required")]
    [StringLength(512, ErrorMessage = "Work item title must be 512 characters or less")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [Required(ErrorMessage = "Work item status is required")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WorkItemStatus Status { get; set; }

    [JsonPropertyName("assignee")]
    [StringLength(256, ErrorMessage = "Assignee name must be 256 characters or less")]
    public string? Assignee { get; set; }
}

public enum WorkItemStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}