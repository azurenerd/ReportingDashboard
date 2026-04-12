using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class WorkItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty; // "Shipped", "InProgress", "CarriedOver"

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("storyPoints")]
    public int StoryPoints { get; set; }

    [JsonPropertyName("completedDate")]
    public DateTime? CompletedDate { get; set; }
}