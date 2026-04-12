using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services
{
    /// <summary>
    /// Service for color codes, CSS class names, and SVG shape generation.
    /// Stub implementation: complete implementation deferred to subsequent PR.
    /// </summary>
    public class VisualizationService : IVisualizationService
    {
        /// <summary>
        /// Get CSS class name for a heatmap cell based on status type.
        /// Stub: implementation deferred.
        /// </summary>
        public string GetCellClassName(string status, bool isCurrentMonth)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get hex color code for item indicator dot based on status type.
        /// Stub: implementation deferred.
        /// </summary>
        public string GetDotColor(string status)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get CSS class name for status row header.
        /// Stub: implementation deferred.
        /// </summary>
        public string GetStatusHeaderClassName(string status)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Generate SVG diamond shape (rotated square) for milestone endpoints.
        /// Stub: implementation deferred.
        /// </summary>
        public string GenerateSvgDiamond(int cx, int cy, string fill, bool withFilter = true)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Generate SVG circle for milestone start/checkpoint markers.
        /// Stub: implementation deferred.
        /// </summary>
        public string GenerateSvgCircle(int cx, int cy, int radius, string fill, string stroke, int strokeWidth)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Generate SVG line for milestone timelines or gridlines.
        /// Stub: implementation deferred.
        /// </summary>
        public string GenerateSvgLine(int x1, int y1, int x2, int y2, string stroke, int strokeWidth, string? dasharray = null)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get visualization metadata for all milestone types.
        /// Stub: implementation deferred.
        /// </summary>
        public Dictionary<string, MilestoneShapeInfo> GetMilestoneShapes()
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }

        /// <summary>
        /// Get hex color for milestone type.
        /// Stub: implementation deferred.
        /// </summary>
        public string GetMilestoneColor(string type)
        {
            throw new NotImplementedException("Implementation deferred to service implementation PR");
        }
    }
}