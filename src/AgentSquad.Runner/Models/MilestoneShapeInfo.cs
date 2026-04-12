namespace AgentSquad.Runner.Models
{
    /// <summary>
    /// Metadata for rendering a milestone shape in SVG timeline.
    /// Returned by VisualizationService.GetMilestoneShapes().
    /// </summary>
    public class MilestoneShapeInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Shape { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Size { get; set; }
    }
}