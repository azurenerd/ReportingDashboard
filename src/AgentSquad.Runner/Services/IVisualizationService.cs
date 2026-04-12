using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service for color codes, CSS class names, and SVG shape generation
/// </summary>
public interface IVisualizationService
{
    /// <summary>
    /// Get the CSS class name for a heatmap cell based on status type
    /// </summary>
    /// <param name="status">Status type ("shipped", "inProgress", "carryover", "blockers")</param>
    /// <param name="isCurrentMonth">Whether cell is in current month column</param>
    /// <returns>CSS class name(s)</returns>
    string GetCellClassName(string status, bool isCurrentMonth);

    /// <summary>
    /// Get the hex color code for an item indicator dot
    /// </summary>
    /// <param name="status">Status type</param>
    /// <returns>Hex color code (e.g., "#34A853")</returns>
    string GetDotColor(string status);

    /// <summary>
    /// Get CSS class name for status row header
    /// </summary>
    /// <param name="status">Status type</param>
    /// <returns>CSS class name</returns>
    string GetStatusHeaderClassName(string status);

    /// <summary>
    /// Generate SVG diamond shape markup
    /// </summary>
    /// <param name="cx">Center x coordinate</param>
    /// <param name="cy">Center y coordinate</param>
    /// <param name="fill">Fill color (hex)</param>
    /// <param name="withFilter">Whether to include drop-shadow filter</param>
    /// <returns>SVG polygon element as string</returns>
    string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true);

    /// <summary>
    /// Generate SVG circle shape markup
    /// </summary>
    /// <param name="cx">Center x coordinate</param>
    /// <param name="cy">Center y coordinate</param>
    /// <param name="radius">Circle radius in pixels</param>
    /// <param name="fill">Fill color (hex)</param>
    /// <param name="stroke">Stroke color (hex)</param>
    /// <param name="strokeWidth">Stroke width in pixels</param>
    /// <returns>SVG circle element as string</returns>
    string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth);

    /// <summary>
    /// Generate SVG line shape markup
    /// </summary>
    /// <param name="x1">Start x coordinate</param>
    /// <param name="y1">Start y coordinate</param>
    /// <param name="x2">End x coordinate</param>
    /// <param name="y2">End y coordinate</param>
    /// <param name="stroke">Stroke color (hex)</param>
    /// <param name="strokeWidth">Stroke width in pixels</param>
    /// <param name="dasharray">Optional SVG stroke-dasharray value (e.g., "5,3")</param>
    /// <returns>SVG line element as string</returns>
    string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null);

    /// <summary>
    /// Get mapping of milestone types to shape/color info
    /// </summary>
    /// <returns>Dictionary of milestone type to MilestoneShapeInfo</returns>
    Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes();
}