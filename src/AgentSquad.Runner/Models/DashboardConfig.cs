using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Root configuration object deserialized from data.json.
    /// Contains project metadata and collections of quarters and milestones.
    /// </summary>
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
}