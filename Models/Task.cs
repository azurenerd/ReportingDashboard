namespace AgentSquad.Models;

public class Task
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssignedOwner { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
}

public enum TaskStatus
{
    Shipped = 0,
    InProgress = 1,
    CarriedOver = 2
}