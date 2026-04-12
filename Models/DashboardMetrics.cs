namespace AgentSquad.Runner.Models
{
    public class DashboardMetrics
    {
        public decimal CompletionPercentage { get; set; }
        public int ShippedCount { get; set; }
        public int CarriedOverCount { get; set; }
        public int TotalWorkItems { get; set; }
        public int InProgressCount { get; set; }
        public int NewCount { get; set; }
    }
}