using System;
using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Models
{
    public class Milestone
    {
        [Required(ErrorMessage = "Milestone name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Milestone name must be between 1 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Target date is required")]
        public DateTime TargetDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public MilestoneStatus Status { get; set; }

        [Required(ErrorMessage = "Completion percentage is required")]
        [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
        public int CompletionPercentage { get; set; }
    }
}