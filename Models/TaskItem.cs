namespace AgentSquad.Models
{
    public class TaskItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } // Shipped, InProgress, CarriedOver
        public string AssignedTo { get; set; }
    }
}