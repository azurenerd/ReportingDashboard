using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class WorkItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WorkItemCategory Category { get; set; }

    [JsonPropertyName("milestoneId")]
    public string? MilestoneId { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemCategory
{
    Shipped,
    InProgress,
    CarriedOver
}