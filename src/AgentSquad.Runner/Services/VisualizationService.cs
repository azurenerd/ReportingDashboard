#nullable enable

using AgentSquad.Runner.Config;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for visualization utilities including color mappings, CSS class names, and SVG generation.
/// Centralizes all visual styling logic to maintain consistency across components.
/// </summary>
public class VisualizationService : IVisualizationService
{
    private readonly ILogger<VisualizationService> logger;

    public VisualizationService(ILogger<VisualizationService> logger)
    {
        this.logger = logger;
    }

    public string GetCellClassName(string status, bool isCurrentMonth)
    {
        return VisualizationConstants.GetCellCssClass(status, isCurrentMonth);
    }

    public string GetDotColor(string status)
    {
        return VisualizationConstants.GetStatusDotColor(status);
    }

    public string GetStatusHeaderClassName(string status)
    {
        return VisualizationConstants.GetRowHeaderCssClass(status);
    }

    public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true)
    {
        const int half = 6;
        var filterAttribute = withFilter ? " filter=\"url(#drop-shadow)\"" : "";

        return $"<polygon points=\"{cx},{cy - half} {cx + half},{cy} {cx},{cy + half} {cx - half},{cy}\" " +
               $"fill=\"{fill}\" stroke=\"{fill}\" stroke-width=\"1\"{filterAttribute} />";
    }

    public string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth)
    {
        return $"<circle cx=\"{cx}\" cy=\"{cy}\" r=\"{radius}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />";
    }

    public string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null)
    {
        var dasharrayAttribute = !string.IsNullOrEmpty(dasharray) ? $" stroke-dasharray=\"{dasharray}\"" : "";
        return $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\"{dasharrayAttribute} />";
    }

    public List<string> GetMilestoneShapes(List<Milestone> milestones, DateTime baselineDate, string nowMarkerColor)
    {
        var shapes = new List<string>();

        if (milestones == null || milestones.Count == 0)
        {
            return shapes;
        }

        try
        {
            var dateCalculationService = new DateCalculationService(
                LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DateCalculationService>()
            );

            foreach (var milestone in milestones)
            {
                if (!DateTime.TryParse(milestone.Date, out var milestoneDate))
                {
                    logger.LogWarning("Invalid milestone date format: {Date}", milestone.Date);
                    continue;
                }

                var xPosition = dateCalculationService.GetMilestoneXPosition(milestoneDate, baselineDate);
                var color = VisualizationConstants.GetMilestoneColor(milestone.Type);
                var cy = 90;

                var shape = milestone.Type switch
                {
                    VisualizationConstants.MilestoneTypeCheckpoint =>
                        GenerateSvgCircle(xPosition, cy, VisualizationConstants.MilestoneCheckpointCircleRadius, color, color, 1),
                    _ =>
                        GenerateSvgDiamond(xPosition, cy, color, true)
                };

                shapes.Add(shape);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating milestone shapes");
        }

        return shapes;
    }
}