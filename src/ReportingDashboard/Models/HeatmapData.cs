namespace ReportingDashboard.Models;

public record HeatmapData
{
    public Dictionary<string, List<string>> Shipped { get; init; } = new();
    public Dictionary<string, List<string>> InProgress { get; init; } = new();
    public Dictionary<string, List<string>> Carryover { get; init; } = new();
    public Dictionary<string, List<string>> Blockers { get; init; } = new();
}