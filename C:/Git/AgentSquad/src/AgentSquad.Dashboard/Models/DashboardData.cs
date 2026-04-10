namespace AgentSquad.Dashboard.Models;

public class DashboardData
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public List<Milestone> Milestones { get; set; } = new();
    public List<StatusItem> Completed { get; set; } = new();
    public List<StatusItem> InProgress { get; set; } = new();
    public List<StatusItem> CarriedOver { get; set; } = new();

    public int CompletedCount => Completed?.Count ?? 0;
    public int InProgressCount => InProgress?.Count ?? 0;
    public int CarriedOverCount => CarriedOver?.Count ?? 0;
    public int TotalCount => CompletedCount + InProgressCount + CarriedOverCount;
}

public class Milestone : IComparable<Milestone>
{
    public string Name { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public bool Completed { get; set; }
    public string Status { get; set; } = "On Track";

    public int CompareTo(Milestone? other) => TargetDate.CompareTo(other?.TargetDate);
}

public class StatusItem : IComparable<StatusItem>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime AddedDate { get; set; }
    public string Owner { get; set; } = string.Empty;

    public int CompareTo(StatusItem? other) => AddedDate.CompareTo(other?.AddedDate);
}