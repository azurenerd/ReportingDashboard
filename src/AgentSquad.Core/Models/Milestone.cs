using System;
using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Core.Models
{
    public class Milestone
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [RegularExpression(@"^(Completed|On Track|At Risk)$")]
        public string Status { get; set; }
    }
}