using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class CalculationService : ICalculationService
{
    public ProjectCalculations CalculateMetrics(ProjectData data)
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

        return new ProjectCalculations
        {
            PercentageComplete = percentageComplete,
            TotalStoryPoints = totalStoryPoints,
            ShippedCount = shippedItems.Count,
            InProgressCount = inProgressItems.Count,
            CarriedOverCount = carriedOverItems.Count,
            VelocityPerSprint = data.Metrics.VelocityPerSprint
        };
    }
}