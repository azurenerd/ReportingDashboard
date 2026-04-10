using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    public class ProjectStatus
    {
        public List<Milestone> Milestones { get; set; } = new();
        public List<Models.Task> Tasks { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}