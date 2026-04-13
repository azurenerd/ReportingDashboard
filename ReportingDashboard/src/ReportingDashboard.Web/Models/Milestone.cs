using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("completionDate")]
    public DateTime? CompletionDate { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MilestoneStatus Status { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneStatus
{
    Completed,
    InProgress,
    Upcoming
}