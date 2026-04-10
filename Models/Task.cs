namespace AgentSquad.Runner.Models
{
    public class Task
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.CarriedOver;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public bool IsOnTime { get; set; } = true;
    }

    public enum TaskStatus
    {
        Completed,
        InProgress,
        CarriedOver
    }
}