using System.Text.Json.Serialization;

namespace AgentSquad.Dashboard.Services;

public class ProjectData
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = "";

    [JsonPropertyName("period")]
    public string Period { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();

    [JsonPropertyName("workItems")]
    public WorkItemAggregation WorkItems { get; set; } = new();
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("completionDate")]
    public DateTime? CompletionDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "upcoming";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("owners")]
    public List<string>? Owners { get; set; }
}

public class WorkItemAggregation
{
    [JsonPropertyName("shipped")]
    public int Shipped { get; set; }

    [JsonPropertyName("inProgress")]
    public int InProgress { get; set; }

    [JsonPropertyName("carriedOver")]
    public int CarriedOver { get; set; }

    [JsonPropertyName("atRisk")]
    public int AtRisk { get; set; }

    [JsonPropertyName("totalCapacity")]
    public int TotalCapacity { get; set; }
}