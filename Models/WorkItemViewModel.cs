using System;

namespace AgentSquad.Runner.Models
{
    public class WorkItemViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public string Owner { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
        public string RowCssClass { get; set; }
        public string RiskBadge { get; set; }
    }
}