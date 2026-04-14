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
    public HeatmapData Heatmap { get; init; } = Heatmap ?? new HeatmapData(
        new HeatmapRow(new Dictionary<string, string[]>()),
        new HeatmapRow(new Dictionary<string, string[]>()),
        new HeatmapRow(new Dictionary<string, string[]>()),
        new HeatmapRow(new Dictionary<string, string[]>()));

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
)
{
    public HeatmapRow Shipped { get; init; } = Shipped ?? new HeatmapRow(new Dictionary<string, string[]>());
    public HeatmapRow InProgress { get; init; } = InProgress ?? new HeatmapRow(new Dictionary<string, string[]>());
    public HeatmapRow Carryover { get; init; } = Carryover ?? new HeatmapRow(new Dictionary<string, string[]>());
    public HeatmapRow Blockers { get; init; } = Blockers ?? new HeatmapRow(new Dictionary<string, string[]>());
}

public record HeatmapRow(
    Dictionary<string, string[]> Items
)
{
    public Dictionary<string, string[]> Items { get; init; } = Items ?? new Dictionary<string, string[]>();

    public string[] GetItems(string month) =>
        Items.TryGetValue(month, out var list) ? list : Array.Empty<string>();

    public int TotalItemCount => Items.Values.Sum(v => v.Length);
}