namespace AgentSquad.Runner.Configuration;

public class DashboardOptions
{
    public string DataJsonPath { get; set; } = "data.json";
    
    public int FileWatchDebounceMs { get; set; } = 500;
}