namespace AgentSquad.Runner.Models;

/// <summary>
/// Calculated metadata about a month in the display timeline
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