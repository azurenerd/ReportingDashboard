namespace AgentSquad.Runner.Models
{
    public class ProjectMetrics
    {
        public int CompletionPercentage { get; set; }
        public HealthStatus HealthStatus { get; set; }
        public int VelocityThisMonth { get; set; }
        public int VelocityLastMonth { get; set; }

        public string GetStatusColor()
        {
            return HealthStatus switch
            {
                HealthStatus.OnTrack => "#28a745",
                HealthStatus.AtRisk => "#dc3545",
                HealthStatus.Blocked => "#6c757d",
                _ => "#6c757d"
            };
        }

        public string GetStatusLabel()
        {
            return HealthStatus switch
            {
                HealthStatus.OnTrack => "On Track",
                HealthStatus.AtRisk => "At Risk",
                HealthStatus.Blocked => "Blocked",
                _ => "Unknown"
            };
        }
    }
}