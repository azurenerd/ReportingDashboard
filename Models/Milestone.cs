using System;

namespace AgentSquad.Runner.Models
{
    public enum MilestoneStatus
    {
        OnTrack,
        AtRisk,
        Completed
    }

    public class Milestone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public MilestoneStatus Status { get; set; }
    }
}