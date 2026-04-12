using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class VisualizationService : IVisualizationService
{
    private const string ColorShippedDot = "#34A853";
    private const string ColorProgressDot = "#0078D4";
    private const string ColorCarryoverDot = "#F4B400";
    private const string ColorBlockersDot = "#EA4335";
    private const string ColorPoC = "#F4B400";
    private const string ColorRelease = "#34A853";
    private const string ColorCheckpoint = "#999";

    public string GetCellClassName(string status, bool isCurrentMonth)
    {
        var baseClass = status.ToLower() switch
        {
            "shipped" => "ship-cell",
            "inprogress" => "prog-cell",
            "carryover" => "carry-cell",
            "blockers" => "block-cell",
            _ => "ship-cell"
        };

        return isCurrentMonth ? $"{baseClass} apr" : baseClass;
    }

    public string GetDotColor(string status)
    {
        return status.ToLower() switch
        {
            "shipped" => ColorShippedDot,
            "inprogress" => ColorProgressDot,
            "carryover" => ColorCarryoverDot,
            "blockers" => ColorBlockersDot,
            _ => ColorShippedDot
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
            _ => "ship-hdr"
        };
    }

    public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true)
    {
        int offset = 8;
        string filterAttr = withFilter ? " filter=\"url(#dropShadow)\"" : "";
        
        return $"<polygon points=\"{cx},{cy - offset} {cx + offset},{cy} {cx},{cy + offset} {cx - offset},{cy}\" " +
               $"fill=\"{fill}\" stroke=\"none\"{filterAttr} />";
    }

    public string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth)
    {
        return $"<circle cx=\"{cx}\" cy=\"{cy}\" r=\"{radius}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />";
    }

    public string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null)
    {
        string dasharrayAttr = !string.IsNullOrEmpty(dasharray) ? $" stroke-dasharray=\"{dasharray}\"" : "";
        return $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\"{dasharrayAttr} />";
    }

    public Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes()
    {
        return new Dictionary<string, MilestoneShapeInfo>
        {
            {
                "poc",
                new MilestoneShapeInfo
                {
                    Type = "poc",
                    Shape = "diamond",
                    Color = ColorPoC,
                    Size = 12
                }
            },
            {
                "release",
                new MilestoneShapeInfo
                {
                    Type = "release",
                    Shape = "diamond",
                    Color = ColorRelease,
                    Size = 12
                }
            },
            {
                "checkpoint",
                new MilestoneShapeInfo
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