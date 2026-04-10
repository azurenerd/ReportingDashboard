using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Core.Models
{
    public class Project
    {
        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }
    }
}