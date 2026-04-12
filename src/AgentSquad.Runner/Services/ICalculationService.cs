using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface ICalculationService
{
    ProjectCalculations CalculateMetrics(ProjectData data);
}