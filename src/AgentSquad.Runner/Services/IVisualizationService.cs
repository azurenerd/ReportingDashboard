using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service interface for color codes, CSS class names, and SVG shape generation.
    /// Centralizes all visual/style definitions for consistency across components.
    /// </summary>
    public interface IVisualizationService
    {
        /// <summary>
        /// Get CSS class name for a heatmap cell based on status type.
        /// </summary>
        /// <param name="status">Status type: "shipped", "inProgress", "carryover", or "blockers".</param>
        /// <param name="isCurrentMonth">Whether the cell is in the current month (applies darker shade).</param>
        /// <returns>CSS class name(s) for the cell (e.g., "ship-cell apr").</returns>
        string GetCellClassName(string status, bool isCurrentMonth);

        /// <summary>
        /// Get hex color code for item indicator dot based on status type.
        /// </summary>
        /// <param name="status">Status type: "shipped", "inProgress", "carryover", or "blockers".</param>
        /// <returns>Hex color code (e.g., "#34A853").</returns>
        string GetDotColor(string status);

        /// <summary>
        /// Get CSS class name for status row header.
        /// </summary>
        /// <param name="status">Status type: "shipped", "inProgress", "carryover", or "blockers".</param>
        /// <returns>CSS class name for the row header (e.g., "ship-hdr").</returns>
        string GetStatusHeaderClassName(string status);

        /// <summary>
        /// Generate SVG diamond shape (rotated square) for milestone endpoints.
        /// </summary>
        /// <param name="cx">Center x-coordinate.</param>
        /// <param name="cy">Center y-coordinate.</param>
        /// <param name="fill">Hex fill color.</param>
        /// <param name="withFilter">Whether to apply drop-shadow filter.</param>
        /// <returns>SVG polygon element as string.</returns>
        string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true);

        /// <summary>
        /// Generate SVG circle for milestone start/checkpoint markers.
        /// </summary>
        /// <param name="cx">Center x-coordinate.</param>
        /// <param name="cy">Center y-coordinate.</param>
        /// <param name="radius">Circle radius in pixels.</param>
        /// <param name="fill">Hex fill color.</param>
        /// <param name="stroke">Hex stroke color.</param>
        /// <param name="strokeWidth">Stroke width in pixels.</param>
        /// <returns>SVG circle element as string.</returns>
        string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth);

        /// <summary>
        /// Generate SVG line for milestone timelines or gridlines.
        /// </summary>
        /// <param name="x1">Start x-coordinate.</param>
        /// <param name="y1">Start y-coordinate.</param>
        /// <param name="x2">End x-coordinate.</param>
        /// <param name="y2">End y-coordinate.</param>
        /// <param name="stroke">Hex stroke color.</param>
        /// <param name="strokeWidth">Stroke width in pixels.</param>
        /// <param name="dasharray">Optional SVG stroke-dasharray value (e.g., "5,3" for dashed lines).</param>
        /// <returns>SVG line element as string.</returns>
        string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null);

        /// <summary>
        /// Get visualization metadata for all milestone types (PoC, release, checkpoint).
        /// </summary>
        /// <returns>Dictionary mapping milestone type to MilestoneShapeInfo.</returns>
        Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes();

        /// <summary>
        /// Get hex color for milestone type (PoC, release, or checkpoint).
        /// </summary>
        /// <param name="type">Milestone type: "poc", "release", or "checkpoint".</param>
        /// <returns>Hex color code.</returns>
        string GetMilestoneColor(string type);
    }
}