using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models
{
    public class WorkItem
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public WorkItemStatus Status { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(100)]
        public string AssignedTo { get; set; }
    }

    public enum WorkItemStatus
    {
        ShippedThisMonth,
        InProgress,
        CarriedOver
    }
}