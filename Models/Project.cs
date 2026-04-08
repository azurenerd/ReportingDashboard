using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public List<Milestone> Milestones { get; set; } = new();
        public List<WorkItem> WorkItems { get; set; } = new();
        public ProjectMetrics Metrics { get; set; }
    }
}