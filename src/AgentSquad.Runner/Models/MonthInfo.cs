#nullable enable

namespace AgentSquad.Runner.Models;

/// <summary>
/// Represents information about a display month on the heatmap.
/// Used for calculating month boundaries and determining current month highlighting.
/// </summary>
public class MonthInfo
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int GridColumnIndex { get; set; }
    public bool IsCurrentMonth { get; set; }
}