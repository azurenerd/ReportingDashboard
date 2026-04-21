using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public sealed class DashboardData
{
    public required Project Project { get; init; }
    public required Timeline Timeline { get; init; }
    public required Heatmap Heatmap { get; init; }
    public Theme? Theme { get; init; }
}

public sealed class Project
{
    public required string Title { get; init; }
    public required string Subtitle { get; init; }
    public string? BacklogUrl { get; init; }
    public string BacklogLinkText { get; init; } = "\u2192 ADO Backlog";

    public static Project Placeholder { get; } = new()
    {
        Title = "(data.json error)",
        Subtitle = "see error banner above"
    };
}

public sealed class Timeline
{
    public required DateOnly Start { get; init; }
    public required DateOnly End { get; init; }
    public required IReadOnlyList<TimelineLane> Lanes { get; init; }
}

public sealed class TimelineLane
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public required string Color { get; init; }
    public required IReadOnlyList<Milestone> Milestones { get; init; }
}

public sealed class Milestone
{
    public required DateOnly Date { get; init; }
    public required MilestoneType Type { get; init; }
    public required string Label { get; init; }
    public CaptionPosition? CaptionPosition { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<MilestoneType>))]
public enum MilestoneType { Checkpoint, Poc, Prod }

[JsonConverter(typeof(JsonStringEnumConverter<CaptionPosition>))]
public enum CaptionPosition { Above, Below }

public sealed class Heatmap
{
    public required IReadOnlyList<string> Months { get; init; }
    public int? CurrentMonthIndex { get; init; }
    public int MaxItemsPerCell { get; init; } = 4;
    public required IReadOnlyList<HeatmapRow> Rows { get; init; }
}

public sealed class HeatmapRow
{
    public required HeatmapCategory Category { get; init; }
    public required IReadOnlyList<IReadOnlyList<string>> Cells { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter<HeatmapCategory>))]
public enum HeatmapCategory { Shipped, InProgress, Carryover, Blockers }

public sealed class Theme
{
    public string? Font { get; init; }
}