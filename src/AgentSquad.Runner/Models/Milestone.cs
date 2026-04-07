using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    public class Milestone
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("targetDate")]
        public DateTime TargetDate { get; set; }

        [JsonPropertyName("status")]
        public MilestoneStatus Status { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}