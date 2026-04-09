using System;

namespace AgentSquad.Runner.Models
{
    public class WorkItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string MilestoneId { get; set; }
        public string Owner { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
    }
}