namespace AgentSquad.Runner.Models;

public class StatusRow
{
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, List<string>> MonthlyItems { get; set; } = new();
}