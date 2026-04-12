using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class DashboardConfig
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quarters")]
    public List<Quarter> Quarters { get; set; } = new();

    [JsonPropertyName("milestones")]
    public List<Milestone> Milestones { get; set; } = new();
}

public class Quarter
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("shipped")]
    public List<string> Shipped { get; set; } = new();

    [JsonPropertyName("inProgress")]
    public List<string> InProgress { get; set; } = new();

    [JsonPropertyName("carryover")]
    public List<string> Carryover { get; set; } = new();

    [JsonPropertyName("blockers")]
    public List<string> Blockers { get; set; } = new();
}

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class MonthInfo
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GridColumnIndex { get; set; }
    public bool IsCurrentMonth { get; set; }
}

public class MilestoneShapeInfo
{
    public string Type { get; set; } = string.Empty;
    public string Shape { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Size { get; set; }
}