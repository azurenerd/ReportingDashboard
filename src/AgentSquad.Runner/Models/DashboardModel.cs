using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Root model for dashboard data from data.json
    /// </summary>
    public class DashboardModel
    {
        public ProjectInfo? Project { get; set; }
        public List<Milestone>? Milestones { get; set; }
        public List<StatusRow>? StatusRows { get; set; }
        public string? NowMarker { get; set; }
    }
}