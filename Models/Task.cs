using System;

namespace AgentSquad.Runner.Models
{
    public enum TaskStatus
    {
        Completed,
        InProgress,
        CarriedOver
    }

    public class Task
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskStatus Status { get; set; }
        public string AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
    }
}