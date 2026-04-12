using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Represents a single month/quarter with status categories (shipped, in progress, carryover, blockers).
    /// Part of the quarters array in data.json.
    /// </summary>
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
}