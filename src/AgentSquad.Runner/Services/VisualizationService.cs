using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Implementation of IVisualizationService for color codes, CSS classes, and SVG shapes
/// </summary>
public class VisualizationService : IVisualizationService
{
    // Color constants
    private const string ColorShippedDot = "#34A853";
    private const string ColorInProgressDot = "#0078D4";
    private const string ColorCarryoverDot = "#F4B400";
    private const string ColorBlockersDot = "#EA4335";

    private const string ColorPoC = "#F4B400";
    private const string ColorProduction = "#34A853";
    private const string ColorCheckpoint = "#999";

    public string GetCellClassName(string status, bool isCurrentMonth)
    {
        var baseClass = status.ToLower() switch
        {
            "shipped" => "ship-cell",
            "inprogress" => "prog-cell",
            "carryover" => "carry-cell",
            "blockers" => "block-cell",
            _ => "cell"
        };

        return isCurrentMonth ? $"{baseClass} apr" : baseClass;
    }

    public string GetDotColor(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => ColorShippedDot,
            "inprogress" => ColorInProgressDot,
            "carryover" => ColorCarryoverDot,
            "blockers" => ColorBlockersDot,
            _ => "#999999"
        };
    }

    public string GetStatusHeaderClassName(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => "ship-hdr",
            "inprogress" => "prog-hdr",
            "carryover" => "carry-hdr",
            "blockers" => "block-hdr",
            _ => "hdr"
        };
    }

    public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true)
    {
        var size = 12;
        var halfSize = size / 2;
        
        var points = $"{cx},{cy - halfSize} {cx + halfSize},{cy} {cx},{cy + halfSize} {cx - halfSize},{cy}";
        var filterId = withFilter ? " filter=\"url(#shadow)\"" : "";
        
        return $"<polygon points=\"{points}\" fill=\"{fill}\"{filterId} />";
    }

    public string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth)
    {
        return $"<circle cx=\"{cx}\" cy=\"{cy}\" r=\"{radius}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />";
    }

    public string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null)
    {
        var dasharrayAttr = !string.IsNullOrEmpty(dasharray) ? $" stroke-dasharray=\"{dasharray}\"" : "";
        return $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\"{dasharrayAttr} />";
    }

    public Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes()
    {
        return new Dictionary<string, MilestoneShapeInfo>
        {
            ["poc"] = new MilestoneShapeInfo
            {
                Type = "poc",
                Shape = "diamond",
                Color = ColorPoC,
                Size = 12
            },
            ["release"] = new MilestoneShapeInfo
            {
                Type = "release",
                Shape = "diamond",
                Color = ColorProduction,
                Size = 12
            },
            ["checkpoint"] = new MilestoneShapeInfo
            {
                Type = "checkpoint",
                Shape = "circle",
                Color = ColorCheckpoint,
                Size = 8
            }
        };
    }
}