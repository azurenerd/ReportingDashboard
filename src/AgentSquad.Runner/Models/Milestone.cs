namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Milestone marker on timeline
    /// </summary>
    public class Milestone
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Date { get; set; }
        public string? Type { get; set; }
        public string? Color { get; set; }
    }
}