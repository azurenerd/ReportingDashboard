namespace AgentSquad.Runner.Models;

/// <summary>
/// Visualization metadata for a milestone type (PoC, release, checkpoint)
/// </summary>
public class MilestoneShapeInfo
{
    public string Type { get; set; } = string.Empty; // "poc", "release", "checkpoint"
    public string Shape { get; set; } = string.Empty; // "diamond", "circle"
    public string Color { get; set; } = string.Empty; // Hex color code
    public int Size { get; set; } // Pixels
}