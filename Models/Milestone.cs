using System;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models
{
    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
        public string Description { get; set; }
    }
}