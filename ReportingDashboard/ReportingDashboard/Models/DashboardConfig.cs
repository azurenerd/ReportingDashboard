using System.Text.Json.Serialization;

namespace ReportingDashboard.Models;

public record DashboardConfig
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string BacklogLink { get; init; } = "";
    public string CurrentMonth { get; init; } = "";
    public List<string> Months { get; init; } = new();
    public List<Milestone> Milestones { get; init; } = new();
    public List<HeatmapRow> HeatmapRows { get; init; } = new();
}

public record Milestone
{
    public string Label { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "#888";
    public List<MilestoneEvent> Events { get; init; } = new();
}

public record MilestoneEvent
{
    public string Date { get; init; } = "";
    public string Type { get; init; } = "checkpoint";
    public string Label { get; init; } = "";
}

public record HeatmapRow
{
    public string Category { get; init; } = "";

    [JsonPropertyName("label")]
    public string Label { get; init; } = "";

    public Dictionary<string, List<string>> Items { get; init; } = new();
}