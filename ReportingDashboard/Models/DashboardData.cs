namespace ReportingDashboard.Models;

public record DashboardData(
    string Title,
    string? Subtitle,
    string? BacklogUrl,
    DateOnly CurrentDate,
    string[] Months,
    int CurrentMonthIndex,
    DateOnly? TimelineStart,
    DateOnly? TimelineEnd,
    MilestoneTrack[] MilestoneTracks,
    HeatmapData Heatmap
)
{
    public string Title { get; init; } = Title ?? "Untitled Dashboard";
    public string[] Months { get; init; } = Months ?? Array.Empty<string>();
    public MilestoneTrack[] MilestoneTracks { get; init; } = MilestoneTracks ?? Array.Empty<MilestoneTrack>();
    public HeatmapData Heatmap { get; init; } = Heatmap ?? new HeatmapData(new HeatmapRow(new()), new HeatmapRow(new()), new HeatmapRow(new()), new HeatmapRow(new()));

    public DateOnly EffectiveTimelineStart => TimelineStart ?? new DateOnly(CurrentDate.Year, 1, 1);
    public DateOnly EffectiveTimelineEnd => TimelineEnd ?? new DateOnly(CurrentDate.Year, 6, 30);
}

public record MilestoneTrack(
    string Name,
    string? Description,
    string Color,
    MilestoneEvent[] Events
)
{
    public MilestoneEvent[] Events { get; init; } = Events ?? Array.Empty<MilestoneEvent>();
}

public record MilestoneEvent(
    DateOnly Date,
    string Label,
    string Type
);

public record HeatmapData(
    HeatmapRow Shipped,
    HeatmapRow InProgress,
    HeatmapRow Carryover,
    HeatmapRow Blockers
);

public record HeatmapRow(
    Dictionary<string, string[]> Items
)
{
    public string[] GetItems(string month) =>
        Items.TryGetValue(month, out var list) ? list : Array.Empty<string>();

    public int TotalItemCount => Items.Values.Sum(v => v.Length);
}