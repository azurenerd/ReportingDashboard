using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public class Milestone
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("targetDate")]
    public DateTime TargetDate { get; set; }

    [JsonPropertyName("completionDate")]
    public DateTime? CompletionDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "upcoming";
}