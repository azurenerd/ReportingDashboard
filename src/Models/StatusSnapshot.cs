using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    public class StatusSnapshot
    {
        public List<string> Shipped { get; set; } = new();
        public List<string> InProgress { get; set; } = new();
        public List<string> CarriedOver { get; set; } = new();
    }
}