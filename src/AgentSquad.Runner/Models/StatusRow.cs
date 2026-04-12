using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Status row in heatmap (Shipped, In Progress, Carryover, Blockers)
    /// </summary>
    public class StatusRow
    {
        public string? Label { get; set; }
        public string? Category { get; set; }
        public List<StatusItem>? Items { get; set; }
    }
}