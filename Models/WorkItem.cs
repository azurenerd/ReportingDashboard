using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models;

public class WorkItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("status")]
    public WorkItemStatus Status { get; set; }

    [JsonPropertyName("assignedTo")]
    public string AssignedTo { get; set; }
}