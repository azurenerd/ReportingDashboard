using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ILogger<CalculationService> _logger;

        public CalculationService(ILogger<CalculationService> logger)
        {
            _logger = logger;
        }

        public ProjectCalculations CalculateMetrics(ProjectData data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                var shippedItems = data.WorkItems
                    .Where(w => w.Category == "Shipped")
                    .ToList();

                var inProgressItems = data.WorkItems
                    .Where(w => w.Category == "InProgress")
                    .ToList();

                var carriedOverItems = data.WorkItems
                    .Where(w => w.Category == "CarriedOver")
                    .ToList();

                int totalStoryPoints = data.Metrics.TotalStoryPoints;
                int completedStoryPoints = data.Metrics.CompletedStoryPoints;
                int percentageComplete = totalStoryPoints > 0
                    ? (int)((completedStoryPoints / (double)totalStoryPoints) * 100)
                    : 0;

                var calculations = new ProjectCalculations
                {
                    PercentageComplete = percentageComplete,
                    TotalStoryPoints = totalStoryPoints,
                    ShippedCount = shippedItems.Count,
                    InProgressCount = inProgressItems.Count,
                    CarriedOverCount = carriedOverItems.Count,
                    VelocityPerSprint = data.Metrics.VelocityPerSprint
                };

                _logger.LogInformation("Calculated metrics: {PercentageComplete}% complete, {ShippedCount} shipped", 
                    percentageComplete, shippedItems.Count);

                return calculations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating metrics");
                throw;
            }
        }
    }
}