using System;

namespace AgentSquad.Runner.Models
{
    public class Milestone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string Color { get; set; }
    }
}