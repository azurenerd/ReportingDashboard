using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Core.Models
{
    public class WorkItem
    {
        [Required]
        [StringLength(512)]
        public string Title { get; set; }

        [Required]
        public WorkItemStatus Status { get; set; }

        [StringLength(256)]
        public string Assignee { get; set; }
    }
}