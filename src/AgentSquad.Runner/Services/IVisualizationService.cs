#nullable enable

using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

/// <summary>
/// Service interface for visualization-related utilities.
/// Provides color mappings, CSS class names, and SVG shape generation.
/// </summary>
public interface IVisualizationService
{
    /// <summary>
    /// Get the CSS class name for a heatmap cell based on status and month context.
    /// </summary>
    string GetCellClassName(string status, bool isCurrentMonth);

    /// <summary>
    /// Get the indicator dot color (hex) for a given status type.
    /// </summary>
    string GetDotColor(string status);

    /// <summary>
    /// Get the row header CSS class name for a given status.
    /// </summary>
    string GetStatusHeaderClassName(string status);

    /// <summary>
    /// Generate SVG markup for a diamond shape (used for PoC and production milestones).
    /// </summary>
    string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true);

    /// <summary>
    /// Generate SVG markup for a circle shape (used for checkpoints).
    /// </summary>
    string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth);

    /// <summary>
    /// Generate SVG markup for a line (used for timeline and gridlines).
    /// </summary>
    string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null);

    /// <summary>
    /// Generate SVG shapes for milestone markers (start, checkpoint, end).
    /// </summary>
    List<string> GetMilestoneShapes(List<Milestone> milestones, DateTime baselineDate, string nowMarkerColor);
}