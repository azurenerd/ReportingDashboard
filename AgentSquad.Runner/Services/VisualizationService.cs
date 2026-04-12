namespace AgentSquad.Runner.Services;

public class VisualizationService
{
    // Status row colors
    private const string ColorShippedBackground = "#F0FBF0";
    private const string ColorShippedBackgroundCurrent = "#D8F2DA";
    private const string ColorShippedDot = "#34A853";
    private const string ColorShippedHeader = "#E8F5E9";
    private const string ColorShippedHeaderText = "#1B7A28";

    private const string ColorInProgressBackground = "#EEF4FE";
    private const string ColorInProgressBackgroundCurrent = "#DAE8FB";
    private const string ColorInProgressDot = "#0078D4";
    private const string ColorInProgressHeader = "#E3F2FD";
    private const string ColorInProgressHeaderText = "#1565C0";

    private const string ColorCarryoverBackground = "#FFFDE7";
    private const string ColorCarryoverBackgroundCurrent = "#FFF0B0";
    private const string ColorCarryoverDot = "#F4B400";
    private const string ColorCarryoverHeader = "#FFF8E1";
    private const string ColorCarryoverHeaderText = "#B45309";

    private const string ColorBlockersBackground = "#FFF5F5";
    private const string ColorBlockersBackgroundCurrent = "#FFE4E4";
    private const string ColorBlockersDot = "#EA4335";
    private const string ColorBlockersHeader = "#FEF2F2";
    private const string ColorBlockersHeaderText = "#991B1B";

    // Timeline/Milestone colors
    private const string ColorM1Timeline = "#0078D4";
    private const string ColorM2Timeline = "#00897B";
    private const string ColorM3Timeline = "#546E7A";
    private const string ColorPocMilestone = "#F4B400";
    private const string ColorProductionRelease = "#34A853";
    private const string ColorCheckpoint = "#999";
    private const string ColorNowMarker = "#EA4335";

    // Border and grid colors
    private const string ColorBorder = "#E0E0E0";
    private const string ColorBorderHeavy = "#CCC";
    private const string ColorCurrentMonthHeader = "#FFF0D0";
    private const string ColorCurrentMonthHeaderText = "#C07700";

    public string GetCellClassName(string status, bool isCurrentMonth)
    {
        var baseClass = status.ToLowerInvariant() switch
        {
            "shipped" => "ship-cell",
            "inprogress" or "in-progress" or "in progress" => "prog-cell",
            "carryover" => "carry-cell",
            "blockers" => "block-cell",
            _ => throw new ArgumentException($"Invalid status: {status}")
        };

        return isCurrentMonth ? $"{baseClass} apr" : baseClass;
    }

    public string GetCellBackgroundColor(string status, bool isCurrentMonth)
    {
        return (status.ToLowerInvariant(), isCurrentMonth) switch
        {
            ("shipped", false) => ColorShippedBackground,
            ("shipped", true) => ColorShippedBackgroundCurrent,
            ("inprogress" or "in-progress" or "in progress", false) => ColorInProgressBackground,
            ("inprogress" or "in-progress" or "in progress", true) => ColorInProgressBackgroundCurrent,
            ("carryover", false) => ColorCarryoverBackground,
            ("carryover", true) => ColorCarryoverBackgroundCurrent,
            ("blockers", false) => ColorBlockersBackground,
            ("blockers", true) => ColorBlockersBackgroundCurrent,
            _ => throw new ArgumentException($"Invalid status: {status}")
        };
    }

    public string GetDotColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "shipped" => ColorShippedDot,
            "inprogress" or "in-progress" or "in progress" => ColorInProgressDot,
            "carryover" => ColorCarryoverDot,
            "blockers" => ColorBlockersDot,
            _ => throw new ArgumentException($"Invalid status: {status}")
        };
    }

    public string GetStatusHeaderClassName(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "shipped" => "ship-hdr",
            "inprogress" or "in-progress" or "in progress" => "prog-hdr",
            "carryover" => "carry-hdr",
            "blockers" => "block-hdr",
            _ => throw new ArgumentException($"Invalid status: {status}")
        };
    }

    public string GetStatusHeaderColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "shipped" => ColorShippedHeaderText,
            "inprogress" or "in-progress" or "in progress" => ColorInProgressHeaderText,
            "carryover" => ColorCarryoverHeaderText,
            "blockers" => ColorBlockersHeaderText,
            _ => throw new ArgumentException($"Invalid status: {status}")
        };
    }

    public string GetStatusHeaderBackgroundColor(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "shipped" => ColorShippedHeader,
            "inprogress" or "in-progress" or "in progress" => ColorInProgressHeader,
            "carryover" => ColorCarryoverHeader,
            "blockers" => ColorBlockersHeader,
            _ => throw new ArgumentException($"Invalid status: {status}")
        };
    }

    public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true)
    {
        var size = 12;
        var points = $"{cx},{cy - size} {cx + size},{cy} {cx},{cy + size} {cx - size},{cy}";
        var filterAttr = withFilter ? " filter=\"url(#shadow)\"" : "";

        return $"<polygon points=\"{points}\" fill=\"{fill}\" stroke=\"none\"{filterAttr} />";
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

    public string GenerateSvgText(int x, int y, string text, string fontSize = "12", string color = "#666", string anchor = "middle")
    {
        var escaped = System.Net.WebUtility.HtmlEncode(text);
        return $"<text x=\"{x}\" y=\"{y}\" font-size=\"{fontSize}\" fill=\"{color}\" text-anchor=\"{anchor}\">{escaped}</text>";
    }

    public string GenerateSvgRect(int x, int y, int width, int height, string fill, string stroke, int strokeWidth)
    {
        return $"<rect x=\"{x}\" y=\"{y}\" width=\"{width}\" height=\"{height}\" fill=\"{fill}\" stroke=\"{stroke}\" stroke-width=\"{strokeWidth}\" />";
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
                    Color = ColorPocMilestone,
                    Size = 12
                }
            },
            {
                "release", new MilestoneShapeInfo
                {
                    Type = "release",
                    Shape = "diamond",
                    Color = ColorProductionRelease,
                    Size = 12
                }
            },
            {
                "checkpoint", new MilestoneShapeInfo
                {
                    Type = "checkpoint",
                    Shape = "circle",
                    Color = ColorCheckpoint,
                    Size = 5
                }
            }
        };
    }

    public MilestoneShapeInfo GetMilestoneShape(string type)
    {
        var shapes = GetMilestoneShapes();
        var key = type.ToLowerInvariant();

        if (shapes.TryGetValue(key, out var shape))
        {
            return shape;
        }

        throw new ArgumentException($"Unknown milestone type: {type}");
    }

    public string GetTimelineColor(int milestoneIndex)
    {
        return milestoneIndex switch
        {
            0 => ColorM1Timeline,
            1 => ColorM2Timeline,
            2 => ColorM3Timeline,
            _ => "#999"
        };
    }

    public int GetTimelineYPosition(int milestoneIndex)
    {
        return milestoneIndex switch
        {
            0 => 42,
            1 => 98,
            2 => 154,
            _ => 42
        };
    }
}

public class MilestoneShapeInfo
{
    public string Type { get; set; } = string.Empty;
    public string Shape { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Size { get; set; }
}