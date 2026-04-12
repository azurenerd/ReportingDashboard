using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public interface IVisualizationService
{
    string GetCellClassName(string status, bool isCurrentMonth);
    string GetDotColor(string status);
    string GetStatusHeaderClassName(string status);
    string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true);
    string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth);
    string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null);
    Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes();
}