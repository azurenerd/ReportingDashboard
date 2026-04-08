using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AgentSquad.Models
{
    public class TaskItem
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Task name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Task name must be between 1 and 200 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Task status is required")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TaskStatus Status { get; set; }

        [StringLength(100, ErrorMessage = "Assigned to field must not exceed 100 characters")]
        public string AssignedTo { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters")]
        public string Description { get; set; }
    }
}