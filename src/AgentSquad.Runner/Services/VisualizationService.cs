using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class VisualizationService : IVisualizationService
{
    private const string ColorShippedDot = "#34A853";
    private const string ColorInProgressDot = "#0078D4";
    private const string ColorCarryoverDot = "#F4B400";
    private const string ColorBlockersDot = "#EA4335";

    private const string ColorPoC = "#F4B400";
    private const string ColorProduction = "#34A853";
    private const string ColorCheckpoint = "#999";

    private const string ColorM1 = "#0078D4";
    private const string ColorM2 = "#00897B";
    private const string ColorM3 = "#546E7A";
    private const string ColorNow = "#EA4335";

    private readonly ILogger<VisualizationService> _logger;

    public VisualizationService(ILogger<VisualizationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GetCellClassName(string status, bool isCurrentMonth)
    {
        var baseClass = status.ToLowerInvariant() switch
        {
            "shipped" => "ship-cell",
            "inprogress" => "prog-cell",
            "carryover" => "carry-cell",
            "blockers" => "block-cell",
            _ => "cell-default"
        };

        if (isCurrentMonth)
        {
            baseClass += " apr";
        }

        return baseClass;
    }

    public string GetDotColor(string status) => status.ToLowerInvariant() switch
    {
        "shipped" => ColorShippedDot,
        "inprogress" => ColorInProgressDot,
        "carryover" => ColorCarryoverDot,
        "blockers" => ColorBlockersDot,
        _ => "#999999"
    };

    public string GetStatusHeaderClassName(string status) => status.ToLowerInvariant() switch
    {
        "shipped" => "ship-hdr",
        "inprogress" => "prog-hdr",
        "carryover" => "carry-hdr",
        "blockers" => "block-hdr",
        _ => "hdr-default"
    };

    public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true)
    {
        var filterId = $"shadow-{cx}-{cy}";
        var filterAttr = withFilter ? $" filter=\"url(#{filterId})\"" : "";

        return $"<g><defs><filter id=\"{filterId}\" x=\"-50%\" y=\"-50%\" width=\"200%\" height=\"200%\">" +
               $"<feDropShadow dx=\"1\" dy=\"1\" stdDeviation=\"2\" flood-opacity=\"0.3\" />" +
               $"</filter></defs>" +
               $"<polygon points=\"{cx},{cy - 8} {cx + 8},{cy} {cx},{cy + 8} {cx - 8},{cy}\" " +
               $"fill=\"{fill}\"{filterAttr} /></g>";
    }

    public string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth)
    {
        return $"<circle cx=\"{cx}\" cy=\"{cy}\" r=\"{radius}\" fill=\"{fill}\" " +
               $"stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />";
    }

    public string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null)
    {
        var dasharrayAttr = !string.IsNullOrEmpty(dasharray) ? $" stroke-dasharray=\"{dasharray}\"" : "";
        return $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{stroke}\" " +
               $"stroke-width=\"{strokeWidth}\"{dasharrayAttr} />";
    }

    public Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes()
    {
        return new Dictionary<string, MilestoneShapeInfo>
        {
            {
                "poc", new MilestoneShapeInfo
                {
                    Type = "poc",
                    Shape = "diamond",
                    Color = ColorPoC,
                    Size = 12
                }
            },
            {
                "release", new MilestoneShapeInfo
                {
                    Type = "release",
                    Shape = "diamond",
                    Color = ColorProduction,
                    Size = 12
                }
            },
            {
                "checkpoint", new MilestoneShapeInfo
                {
                    Type = "checkpoint",
                    Shape = "circle",
                    Color = ColorCheckpoint,
                    Size = 8
                }
            }
        };
    }
}