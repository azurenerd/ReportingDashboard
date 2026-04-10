using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Core.Models
{
    public class DashboardData
    {
        [Required]
        public Project Project { get; set; }

        public List<Milestone> Milestones { get; set; } = new();

        public List<WorkItem> WorkItems { get; set; } = new();
    }
}