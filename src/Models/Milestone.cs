namespace AgentSquad.Runner.Models
{
    public class Milestone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TargetDate { get; set; }
        public string Status { get; set; }
        public int Progress { get; set; }
        public string Description { get; set; }
    }
}