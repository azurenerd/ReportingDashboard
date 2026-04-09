using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    public class ProjectReport
    {
        public string ProjectName { get; set; }
        public string ReportingPeriod { get; set; }
        public Milestone[] Milestones { get; set; }
        public StatusSnapshot StatusSnapshot { get; set; }
        public Dictionary<string, int> Kpis { get; set; }
    }
}