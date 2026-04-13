using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class WorkItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public WorkItemCategory Category { get; set; } = WorkItemCategory.InProgress;

    [JsonPropertyName("priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkItemCategory
{
    Shipped,
    InProgress,
    CarriedOver,
    Blocked
}