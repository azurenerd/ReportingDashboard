using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class Milestone
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("targetDate")]
    public string TargetDate { get; set; } = string.Empty;

    [JsonPropertyName("completionDate")]
    public string? CompletionDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Upcoming";
}